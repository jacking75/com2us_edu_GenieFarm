using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Bcpg;
using SqlKata;
using ZLogger;

public class AttendanceService : IAttendanceService
{
    ILogger<AttendanceService> _logger;
    IGameDb _gameDb;
    IMasterDb _masterDb;

    public AttendanceService(ILogger<AttendanceService> logger, IGameDb gameDb, IMasterDb masterDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
    }

    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceData(Int64 userId)
    {
        // 출석 정보 로드
        var attendanceData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (attendanceData == null)
        {
            return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.AttendanceService_GetAttendanceData, null);
        }

        return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.None, attendanceData);
    }

    public async Task<ErrorCode> Attend(long userId, AttendanceModel attendanceData)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 누적 출석일 수 계산
        var newCount = CalcAttendanceCount(attendanceData);

        // 출석 체크
        var attendResult = await UpdateAttendanceData(userId, newCount, attendanceData, rollbackQueries);
        if (!SuccessOrLogDebug(attendResult, new { UserId = userId, NewAttendanceCount = newCount }))
        {
            return ErrorCode.AttendanceService_UpdateAttendanceData;
        }

        // 보상 지급
        var rewardResult = await SendAttendanceReward(userId, newCount, rollbackQueries);
        if (!SuccessOrLogDebug(rewardResult, new { UserID = userId, NewAttendanceCount = newCount }))
        {
            await Rollback(rewardResult, rollbackQueries);

            return ErrorCode.AttendanceService_SendAttendanceReward;
        }

        return ErrorCode.None;
    }

    Int32 CalcAttendanceCount(AttendanceModel attendanceData)
    {
        // 최대 누적일수를 넘었다면 1로 초기화
        if (attendanceData.AttendanceCount >= _masterDb._definedValueDictionary!["Max_Attendance_Count"])
        {
            return 1;
        }

        // 그 외에는 1 증가
        return attendanceData.AttendanceCount + 1;
    }

    async Task<ErrorCode> UpdateAttendanceData(Int64 userId, Int32 newAttendanceCount, AttendanceModel attendanceData, List<SqlKata.Query> queries)
    {
        try
        {
            // 출석 카운트 갱신
            var affectedRow = await _gameDb.UpdateAttendanceData(userId, newAttendanceCount);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AttendanceService_UpdateAttendanceData_AffectedRowOutOfRange;
            }

            // 성공 시 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_attendance").Where("user_id", userId)
                               .AsUpdate(new { LastAttendance = attendanceData.LastAttendance,
                                               AttendanceCount = attendanceData.AttendanceCount });
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AttendanceService_UpdateAttendanceData_Fail;

            _logger.ZLogErrorWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId, NewAttendanceCount = newAttendanceCount }, "Failed");

            return errorCode;
        }
    }

    async Task<ErrorCode> SendAttendanceReward(Int64 userId, Int32 newAttendanceCount, List<SqlKata.Query> queries)
    {
        // MasterDB에서 보상 아이템 데이터 가져오기
        var reward = _masterDb._attendanceRewardList![newAttendanceCount - 1];

        // 보상 아이템 생성
        (var createResult, var itemId) = await CreateItemAndGetId(reward.ItemCode, reward.Count, queries);
        if (!SuccessOrLogDebug(createResult, new { UserID = userId, Reward = reward }))
        {
            return ErrorCode.AttendanceService_SendAttendanceReward_CreateItem;
        }

        // 출석 보상 지급
        var sendResult = await SendRewardIntoMail(userId, newAttendanceCount, itemId, reward.Money);
        if (!SuccessOrLogDebug(sendResult, new { UserId = userId, NewAttendanceCount = newAttendanceCount }))
        {
            return ErrorCode.AttendanceService_SendAttendanceReward_SendRewardIntoMail;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, Int64>> CreateItemAndGetId(Int64 itemCode, Int16 itemCount, List<SqlKata.Query> queries)
    {
        // 보상이 아이템이 아닌 경우
        if (itemCode == 0)
        {
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, 0);
        }

        try
        {
            // 아이템 생성
            var itemId = await _gameDb.InsertGetIdNewItem(itemCode, itemCount);

            // 생성된 아이템에 대한 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_item").Where("ItemId", itemId).AsDelete();
            queries.Add(query);

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, itemId);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AttendanceService_CreateItem_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { ItemCode = itemCode, ItemCount = itemCount }, "Failed");

            return new Tuple<ErrorCode, Int64>(errorCode, 0);
        }
    }

    async Task<ErrorCode> SendRewardIntoMail(Int64 userId, Int32 newAttendanceCount, Int64 itemId, Int32 money)
    {
        // 메일 생성
        var mail = AttendanceRewardMailGenerator.Create(receiverId: userId,
                                                        senderId: _masterDb._definedValueDictionary!["AttendReward_SenderId"],
                                                        attendanceCount: newAttendanceCount,
                                                        expiry: _masterDb._definedValueDictionary!["AttendReward_Expiry"],
                                                        itemId: itemId,
                                                        money: money);

        try
        {
            // 메일 발송
            var insertedRow = await _gameDb.InsertAttendanceRewardMail(userId, mail);
            if (!ValidateAffectedRow(insertedRow, 1))
            {
                return ErrorCode.AttendanceService_SendRewardIntoMail_InsertedRowOutOfRange;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AttendanceService_SendRewardIntoMail_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId, ItemID = itemId, Money = money }, "Failed");

            return errorCode;
        }
    }

    bool SuccessOrLogDebug<TPayload>(ErrorCode errorCode, TPayload payload)
    {
        if (errorCode == ErrorCode.None)
        {
            return true;
        }
        else
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), payload, "Failed");
            return false;
        }
    }

    async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> queries)
    {
        await _gameDb.Rollback(errorCode, queries);
    }

    bool ValidateAffectedRow(Int32 affectedRow, Int32 expected)
    {
        return affectedRow == expected;
    }
}
