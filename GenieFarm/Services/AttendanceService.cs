using MySqlX.XDevAPI.Common;
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
    /// 유저ID를 기준으로 출석 데이터를 로드하고, <br/>
    /// 출석할 수 있는지 체크합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceData(Int64 userId)
    {
        // 출석 정보 로드
        var attendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (attendData == null)
        {
            return new (ErrorCode.AttendanceService_GetAttendanceData, null);
        }

        // 출석 가능 여부 체크
        if (!ValidateLastAttendance(attendData))
        {
            return new (ErrorCode.AttendanceService_ValidateLastAttendance, null);
        }

        return new (ErrorCode.None, attendData);
    }

    /// <summary>
    /// 유저ID를 현재 시각 기준으로 출석 처리합니다.
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

        // 보상 아이템 우편으로 지급
        var rewardResult = await SendRewardMail(userId, newAttendCount, usingPass, rollbackQueries);
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
    /// 보상 데이터를 로드해 우편으로 지급합니다.
    /// </summary>
    async Task<ErrorCode> SendRewardMail(Int64 userId, Int32 newAttendCount, bool usingPass, List<SqlKata.Query> queries)
    {
        // 이번 출석에 대한 보상 데이터 로드
        var reward = GetAttendanceReward(newAttendCount, usingPass);

        // 보상 메일 데이터 생성
        var mail = GenerateRewardMail(userId, newAttendCount, reward);

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
                                         new { UserID = userId, Reward = reward }, "Failed");

            return errorCode;
        }
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
    /// 보상 지급 메일(MailModel)을 생성합니다.
    /// </summary>
    MailModel GenerateRewardMail(Int64 receiver, Int32 attendCount, AttendanceRewardModel reward)
    {
        return AttendanceRewardMailGenerator.Create(receiverId: receiver,
                                                    senderId: _masterDb._definedValueDictionary!["AttendReward_SenderId"],
                                                    attendanceCount: attendCount,
                                                    expiry: _masterDb._definedValueDictionary!["AttendReward_Expiry"],
                                                    reward: reward);
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

    /// <summary>
    /// 마지막 출석일로부터 1일 이상 차이가 나면 출석 가능
    /// </summary>
    bool ValidateLastAttendance(AttendanceModel attendData)
    {
        return IsAnotherDay(attendData.LastAttendance);
    }

    /// <summary>
    /// 주어진 날짜가 오늘 날짜와 1일 이상 차이가 나는지 비교
    /// </summary>
    bool IsAnotherDay(DateTime lastAttendDate)
    {
        // 현재 날짜와 마지막 출석 날짜가 1일 이상 차이가 나면 True
        var currentDate = DateTime.Parse(DateTime.Now.ToShortDateString());
        lastAttendDate = DateTime.Parse(lastAttendDate.ToShortDateString());
        var diff = currentDate - lastAttendDate;

        return diff > TimeSpan.FromDays(0);
    }
}
