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

        // 출석 데이터 갱신
        (var updateResult, var attendanceCount) = await UpdateAttendanceData(userId, attendData, rollbackQuerys);
        if (updateResult != ErrorCode.None)
        {
            return ErrorCode.GameDB_Fail_UpdateAttendDataException;
        }

        // 출석 보상 우편함으로 지급
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
            // 출석일수 계산
            var attendanceCount = CalcAttendanceCount(attendData);

            // 출석 처리
            var attendResult = await UpdateLastAttendanceAndCount(userId, attendData, attendanceCount);
            if (attendResult != ErrorCode.None)
            {
                return new Tuple<ErrorCode, Int32>(ErrorCode.GameDB_Fail_UpdatedAttendanceRowOutOfRange, -1);
            }

            // 롤백 쿼리 추가
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
        // 연월이 같다면 누적
        if (attendData.LastAttendance.Year == DateTime.Now.Year &&
            attendData.LastAttendance.Month == DateTime.Now.Month)
        {
            return attendData.AttendanceCount + 1;
        }
        else
        {
            // 달라졌다면 1로 초기화
            return 1;
        }
    }

    async Task<ErrorCode> UpdateLastAttendanceAndCount(Int64 userId, AttendanceModel attendData, Int32 attendanceCount)
    {
        // 현재 시간으로 출석 정보 갱신
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
        // 출석 보상 정보 및 구독제 이용여부 확인
        var reward = _masterDb._attendanceRewardList![attendanceCount - 1];
        var usingPass = ValidatePassDate(passEndDate);

        // 구독제 이용시 보상 2배 처리
        if (usingPass)
        {
            reward.Count *= 2;
            reward.Money *= 2;
        }

        // 출석 보상 아이템 생성
        (var createResult, var itemId) = await CreateRewardItem(reward.ItemCode, reward.Count, rollbackQuerys);
        if (createResult != ErrorCode.None)
        {
            return createResult;
        }

        // 출석 보상 지급
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
        // 보상 아이템이 존재하지 않는다면 return
        if (itemCode == 0)
        {
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, 0);
        }

        // 아이템 생성
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

        // 생성 아이템에 대한 롤백 쿼리 추가
        var rollbackQuery = _queryFactory.Query("farm_item").Where("ItemId", itemId).AsDelete();
        rollbackQuerys.Add(rollbackQuery);

        return new Tuple<ErrorCode, Int64>(ErrorCode.None, itemId);
    }

    async Task<bool> SendRewardIntoMail(Int64 userId, Int32 attendanceCount, Int64 itemId, Int64 money)
    {
        try
        {
            // 우편 전송
            var mailId = await _queryFactory.Query("mail_info")
                                            .InsertAsync(new MailModel { SenderId = 0, ReceiverId = userId,
                                                                         Title = $"출석 보상 지급",
                                                                         Content = $"{attendanceCount}일차 출석 보상입니다.",
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