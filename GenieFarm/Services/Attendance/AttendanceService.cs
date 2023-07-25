﻿using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Bcpg;
using SqlKata;
using ZLogger;

/// <summary>
/// 출석체크와 관련된 비즈니스 로직을 처리하고 <br/>
/// DB Operation Call을 수행하는 서비스 클래스
/// </summary>
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

    /// <summary>
    /// 유저ID를 기준으로 출석 데이터를 로드한다.
    /// </summary>
    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceData(Int64 userId)
    {
        // 출석 정보 로드
        var attendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (attendData == null)
        {
            return new (ErrorCode.AttendanceService_GetAttendanceData, null);
        }

        return new (ErrorCode.None, attendData);
    }

    /// <summary>
    /// 유저ID를 현재 시각 기준으로 출석 처리한다.
    /// </summary>
    public async Task<ErrorCode> Attend(long userId, AttendanceModel attendData, bool usingPass)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 누적 출석일 수 계산
        var newAttendCount = CalcAttendanceCount(attendData);

        // 최종 출석일 갱신
        var attendResult = await UpdateAttendanceData(userId, newAttendCount, attendData, rollbackQueries);
        if (!Successed(attendResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(attendResult),
                                         new { UserId = userId, NewAttendCount = newAttendCount }, "Failed");

            return ErrorCode.AttendanceService_UpdateAttendanceData;
        }

        // 보상 아이템 생성 및 우편으로 지급
        var rewardResult = await CreateRewardItemAndSend(userId, newAttendCount, usingPass, rollbackQueries);
        if (!Successed(rewardResult))
        {
            await Rollback(rewardResult, rollbackQueries);

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(rewardResult),
                                         new { UserID = userId, NewAttendCount = newAttendCount }, "Failed");

            return ErrorCode.AttendanceService_SendAttendanceReward;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 누적 출석일수(AttendanceCount)와 <br/>
    /// 최종 출석일(LastAttendance)을 갱신합니다.
    /// </summary>
    async Task<ErrorCode> UpdateAttendanceData(Int64 userId, Int32 newAttendCount, AttendanceModel attendData, List<SqlKata.Query> queries)
    {
        try
        {
            // 누적 출석일수와 최종 출석일 갱신
            var affectedRow = await _gameDb.UpdateAttendanceData(userId, newAttendCount);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AttendanceService_UpdateAttendanceData_AffectedRowOutOfRange;
            }

            // 성공 시 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_attendance").Where("UserId", userId)
                               .AsUpdate(new { LastAttendance = attendData.LastAttendance,
                                               AttendanceCount = attendData.AttendanceCount });
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AttendanceService_UpdateAttendanceData_Fail;

            _logger.ZLogErrorWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId,
                                               NewAttendanceCount = newAttendCount },
                                         "Failed");

            return errorCode;
        }
    }

    /// <summary>
    /// 보상 데이터를 로드하고, 아이템을 생성해서 <br/>
    /// 우편으로 지급합니다.
    /// </summary>
    async Task<ErrorCode> CreateRewardItemAndSend(Int64 userId, Int32 newAttendCount, bool usingPass, List<SqlKata.Query> queries)
    {
        // 이번 출석에 대한 보상 데이터 로드
        var reward = GetAttendanceReward(newAttendCount, usingPass);

        // 보상이 아이템이라면, 생성 후 아이템 ID 가져옴
        // 아이템이 아니라면 itemId는 0
        (var createResult, var itemId) = await CreateItemAndGetId(reward.ItemCode, reward.Count, queries);
        if (!Successed(createResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(createResult),
                                        new { UserID = userId, Reward = reward }, "Failed");

            return ErrorCode.AttendanceService_SendAttendanceReward_CreateItem;
        }

        // 우편함으로 출석 보상 지급
        var sendResult = await SendRewardIntoMail(userId, newAttendCount, itemId, reward.Money);
        if (!Successed(sendResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(sendResult),
                                         new { UserId = userId, NewAttendanceCount = newAttendCount },
                                         "Failed");

            return ErrorCode.AttendanceService_SendAttendanceReward_SendRewardIntoMail;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 유저의 이번 출석 보상 데이터를 반환합니다. <br/>
    /// 마스터DB로부터 보상 데이터를 로드해, <br/>
    /// 월간 구독제를 이용중이라면 보상을 2배 처리합니다.
    /// </summary>
    AttendanceRewardModel GetAttendanceReward(Int32 newAttendCount, bool usingPass)
    {
        var reward = _masterDb._attendanceRewardList![newAttendCount - 1];

        // 월간 구독 중이라면 보상 2배 처리
        if (usingPass)
        {
            reward.Count *= 2;
            reward.Money *= 2;
        }

        return _masterDb._attendanceRewardList![newAttendCount - 1];
    }

    /// <summary>
    /// 아이템을 생성하고, 생성된 아이템 ID를 반환합니다.
    /// </summary>
    async Task<Tuple<ErrorCode, Int64>> CreateItemAndGetId(Int64 itemCode, Int16 itemCount, List<SqlKata.Query> queries)
    {
        // 보상이 아이템이 아닌 경우
        if (!ValidateItemCode(itemCode))
        {
            return new (ErrorCode.None, 0);
        }

        try
        {
            // DB에 아이템 생성 후 아이템 ID 가져옴
            var itemId = await _gameDb.InsertGetIdNewItem(itemCode, itemCount);

            // 생성된 아이템에 대한 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_item")
                               .Where("ItemId", itemId)
                               .AsDelete();
            queries.Add(query);

            return new (ErrorCode.None, itemId);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AttendanceService_CreateItem_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { ItemCode = itemCode, ItemCount = itemCount }, "Failed");

            return new (errorCode, 0);
        }
    }

    /// <summary>
    /// 보상 아이템 혹은 재화를 첨부한 우편을 발송합니다.
    /// </summary>
    async Task<ErrorCode> SendRewardIntoMail(Int64 userId, Int32 newAttendCount, Int64 itemId, Int64 money)
    {
        // 메일 생성
        var mail = GenerateRewardMail(userId, newAttendCount, itemId, money);

        try
        {
            // 메일 발송 처리
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

    /// <summary>
    /// 보상 지급 메일(MailModel)을 생성합니다.
    /// </summary>
    MailModel GenerateRewardMail(Int64 receiver, Int32 attendCount, Int64 rewardItemId, Int64 rewardMoney)
    {
        return AttendanceRewardMailGenerator.Create(receiverId: receiver,
                                                    senderId: _masterDb._definedValueDictionary!["AttendReward_SenderId"],
                                                    attendanceCount: attendCount,
                                                    expiry: _masterDb._definedValueDictionary!["AttendReward_Expiry"],
                                                    itemId: rewardItemId,
                                                    money: rewardMoney);
    }

    /// <summary>
    /// 에러코드가 ErrorCode.None이면 true를 리턴하고, 아니면 false를 리턴합니다.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }

    /// <summary>
    /// GameDB에 쿼리 롤백을 요청합니다.
    /// </summary>
    async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> queries)
    {
        await _gameDb.Rollback(errorCode, queries);
    }

    /// <summary>
    /// Update, Insert, Delete 쿼리의 영향을 받은 행의 개수가 <br/>
    /// 기대한 값과 동일한지 판단해 true, false를 리턴합니다.
    /// </summary>
    bool ValidateAffectedRow(Int32 affectedRow, Int32 expected)
    {
        return affectedRow == expected;
    }

    /// <summary>
    /// 아이템 코드 유효성을 검사합니다.
    /// </summary>
    bool ValidateItemCode(Int64 itemCode)
    {
        return itemCode != 0;
    }

    /// <summary>
    /// 유저ID를 이용해 월간 구독 이용권 만료일을 조회하고, <br/>
    /// 이용권이 유효하면 true를 반환한다.
    /// </summary>
    public async Task<bool> CheckUsingPass(long userId)
    {
        var passEndDate = await _gameDb.GetPassEndDateByUserId(userId);

        // 만료일이 지나지 않았다면 유효함
        if (passEndDate > DateTime.Now)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 누적 출석일수가 1일로 초기화되어야 하는지, <br/>
    /// 누적되어야 하는지 결정하여 <br/>
    /// user_attendance 테이블에 Update될 누적 출석일수 값을 반환합니다.
    /// </summary>
    Int32 CalcAttendanceCount(AttendanceModel attendData)
    {
        // 최대 출석 누적일수를 넘겼다면 1로 초기화
        if (attendData.AttendanceCount >= _masterDb._definedValueDictionary!["Max_Attendance_Count"])
        {
            return 1;
        }

        // 그 외에는 누적
        return attendData.AttendanceCount + 1;
    }
}
