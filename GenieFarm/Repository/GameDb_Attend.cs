using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;
using Org.BouncyCastle.Bcpg;

public partial class GameDb : IGameDb
{
    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceDataByUserId(Int64 userId)
    {
        var result = await GetAttendData(userId);
        if (result == null)
        {
            return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.GameDB_Fail_AttendDataNotExistsByUserID, null);
        }

        return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.None, result);
    }

    public async Task<ErrorCode> Attend(Int64 userId, AttendanceModel attendData)
    {
        var rollbackQuerys = new List<SqlKata.Query>();

        // �⼮ ������ ����
        (var updateResult, var attendanceCount) = await UpdateAttendanceData(userId, attendData, rollbackQuerys);
        if (updateResult != ErrorCode.None)
        {
            return ErrorCode.GameDB_Fail_UpdateAttendDataException;
        }

        // �⼮ ���� ���������� ����
        var rewardResult = await RewardForAttend(userId, attendanceCount, attendData.PassEndDate, rollbackQuerys);
        if (rewardResult != ErrorCode.None)
        {
            var errorCode = ErrorCode.GameDB_Fail_SendMailAttendRewardException;

            await Rollback(errorCode, rollbackQuerys);

            return errorCode;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, Int32>> UpdateAttendanceData(Int64 userId, AttendanceModel attendData, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            // �⼮�ϼ� ���
            var attendanceCount = CalcAttendanceCount(attendData);

            // �⼮ ó��
            var attendResult = await UpdateLastAttendanceAndCount(userId, attendData, attendanceCount);
            if (attendResult != ErrorCode.None)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.GameDB_Fail_UpdatedAttendanceRowOutOfRange, -1);
            }

            // �ѹ� ���� �߰�
            var rollbackQuery = _queryFactory.Query("user_attendance").Where("UserId", userId)
                                             .AsUpdate(new { LastAttendance = attendData.LastAttendance,
                                                             AttendanceCount = attendData.AttendanceCount });
            rollbackQuerys.Add(rollbackQuery);

            return new Tuple<ErrorCode, Int32>(ErrorCode.None, attendanceCount);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.GameDB_Fail_UpdateAttendDataException;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         ex, new { UserID = userId, AttendData = attendData }, "Failed");

            return new Tuple<ErrorCode, Int32>(errorCode, -1);
        }
    }

    Int32 CalcAttendanceCount(AttendanceModel attendData)
    {
        // ������ ���ٸ� ����
        if (attendData.LastAttendance.Year == DateTime.Now.Year &&
            attendData.LastAttendance.Month == DateTime.Now.Month)
        {
            return attendData.AttendanceCount + 1;
        }
        else
        {
            // �޶����ٸ� 1�� �ʱ�ȭ
            return 1;
        }
    }

    async Task<ErrorCode> UpdateLastAttendanceAndCount(Int64 userId, AttendanceModel attendData, Int32 attendanceCount)
    {
        // ���� �ð����� �⼮ ���� ����
        var updatedRow = await _queryFactory.Query("user_attendance").Where("UserId", userId)
                                            .UpdateAsync(new
                                            {
                                                LastAttendance = DateTime.Now,
                                                AttendanceCount = attendanceCount
                                            });

        if (updatedRow != 1)
        {
            var errorCode = ErrorCode.GameDB_Fail_UpdatedAttendanceRowOutOfRange;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { UserID = userId, AttendData = attendData }, "Failed");

            return ErrorCode.GameDB_Fail_UpdatedAttendanceRowOutOfRange;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> RewardForAttend(Int64 userId, Int32 attendanceCount, DateTime passEndDate, List<SqlKata.Query> rollbackQuerys)
    {
        // �⼮ ���� ���� �� ������ �̿뿩�� Ȯ��
        var reward = _masterDb._attendanceRewardList![attendanceCount - 1];
        var usingPass = ValidatePassDate(passEndDate);

        // ������ �̿�� ���� 2�� ó��
        if (usingPass)
        {
            reward.Count *= 2;
            reward.Money *= 2;
        }

        // �⼮ ���� ������ ����
        (var createResult, var itemId) = await CreateRewardItem(reward.ItemCode, reward.Count, rollbackQuerys);
        if (createResult != ErrorCode.None)
        {
            return createResult;
        }

        // �⼮ ���� ����
        if (!await SendRewardIntoMail(userId, attendanceCount, itemId, reward.Money))
        {
            var errorCode = ErrorCode.GameDB_Fail_SendRewardIntoMailbox;

            return errorCode;
        }

        return ErrorCode.None;
    }

    bool ValidatePassDate(DateTime passEndDate)
    {
        if (DateTime.Now < passEndDate)
        {
            return true;
        }

        return false;
    }

    async Task<Tuple<ErrorCode, Int64>> CreateRewardItem(Int64 itemCode, Int32 itemCount, List<SqlKata.Query> rollbackQuerys)
    {
        // ���� �������� �������� �ʴ´ٸ� return
        if (itemCode == 0)
        {
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, 0);
        }

        // ������ ����
        var itemId = await _queryFactory.Query("farm_item")
                                        .InsertGetIdAsync<Int64>(new { OwnerId = 0,
                                                                       ItemCode = itemCode,
                                                                       ItemCount = itemCount });

        if (itemId == 0)
        {
            var errorCode = ErrorCode.GameDB_Fail_CreateAttendRewardItem;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { ItemCode = itemCode, ItemCount = itemCount }, "Failed");

            return new Tuple<ErrorCode, Int64>(errorCode, 0);
        }

        // ���� �����ۿ� ���� �ѹ� ���� �߰�
        var rollbackQuery = _queryFactory.Query("farm_item").Where("ItemId", itemId).AsDelete();
        rollbackQuerys.Add(rollbackQuery);

        return new Tuple<ErrorCode, Int64>(ErrorCode.None, itemId);
    }

    async Task<bool> SendRewardIntoMail(Int64 userId, Int32 attendanceCount, Int64 itemId, Int64 money)
    {
        try
        {
            // ���� ����
            var mailId = await _queryFactory.Query("mail_info")
                                            .InsertAsync(new MailModel { SenderId = 0, ReceiverId = userId,
                                                                         Title = $"�⼮ ���� ����",
                                                                         Content = $"{attendanceCount}���� �⼮ �����Դϴ�.",
                                                                         IsRead = false, IsDeleted = false,
                                                                         ExpiredAt = DateTime.Now.AddDays(7),
                                                                         IsReceived = false, ItemId = itemId, Gold = money });

            if (mailId == 0)
            {
                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.GameDB_Fail_SendRewardException),
                                             new { UserID = userId, AttendanceCount = attendanceCount,
                                                   ItemID = itemId, Money = money }, "Failed");

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.GameDB_Fail_SendRewardException),
                                         ex, new { UserID = userId, AttendanceCount = attendanceCount,
                                                   ItemID = itemId, Money = money }, "Failed");

            return false;
        }
    }
}