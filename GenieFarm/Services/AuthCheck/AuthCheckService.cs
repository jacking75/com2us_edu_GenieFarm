using SqlKata;
using SqlKata.Execution;
using ZLogger;

public class AuthCheckService : IAuthCheckService
{
    readonly ILogger<AuthCheckService> _logger;
    readonly IGameDb _gameDb;
    readonly IMasterDb _masterDb;
    readonly IRedisDb _redisDb;

    public AuthCheckService(ILogger<AuthCheckService> logger, IGameDb gameDb, IMasterDb masterDb, IRedisDb redisDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
        _redisDb = redisDb;
    }

    public async Task<ErrorCode> CheckPlayerExists(string playerId)
    {
        var userId = await _gameDb.GetUserIdByPlayerId(playerId);
        if (userId == 0)
        {
            return ErrorCode.AuthCheckService_CheckPlayerExists_NotExists;
        }

        return ErrorCode.None;
    }

    public async Task<ErrorCode> CreateDefaultGameData(string playerId, string nickname)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 유저 데이터 생성
        (var defaultUserDataResult, var userId) = await CreateDefaultUserData(playerId, nickname, rollbackQueries);
        if (!SuccessOrLogDebug(defaultUserDataResult, new { PlayerID = playerId, Nickname = nickname }))
        {
            return ErrorCode.AuthCheckService_CreateDefaultGameData_DuplicatedNickname;
        }

        // 출석 데이터 생성
        var attendanceDataResult = await CreateDefaultAttendanceData(userId, rollbackQueries);
        if (!SuccessOrLogDebug(attendanceDataResult, new { UserID = userId }))
        {
            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_AttendData;
        }

        // 농장 기본 데이터 생성
        var farmDataResult = await CreateDefaultFarmData(userId, rollbackQueries);
        if (!SuccessOrLogDebug(farmDataResult, new { UserID = userId }))
        {
            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_FarmData;
        }

        // 기본 아이템 Insert
        var defaultItemResult = await CreateDefaultItems(userId, rollbackQueries);
        if (!SuccessOrLogDebug(defaultItemResult, new { UserID = userId }))
        {
            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_Items;
        }

        return ErrorCode.None;
    }

    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(string playerId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await _gameDb.GetDefaultUserDataByPlayerId(playerId);
        if (result.UserData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_UserData, null);
        }

        // 출석 정보 로드
        var userId = result.UserData.UserId;
        result.AttendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (result.AttendData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_AttendData, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await _gameDb.GetDefaultFarmDataByUserId(userId);
        if (result.FarmInfoData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_FarmData, null);
        }

        return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.None, result);
    }

    public async Task<ErrorCode> UpdateLastLoginAt(Int64 userId)
    {
        try
        {
            var affectedRow = await _gameDb.UpdateLastLoginAt(userId);

            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuthCheckService_UpdateLastLoginAt_AffectedRowOutOfRange;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_UpdateLastLoginAt_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId }, "Failed");

            return errorCode;
        }
    }

    public async Task<ErrorCode> SetTokenOnRedis(Int64 userId, string token)
    {
        // 마스터DB에서 토큰 유효시간 가져옴
        var expiry = _masterDb._definedValueDictionary!["Redis_Token_Expiry_Hour"];

        // 같은 키의 토큰이 있어도 무조건 Overwrite하여 기존 토큰을 무효화
        if (!await _redisDb.SetAsync(userId, token, TimeSpan.FromHours(expiry)))
        {
            return ErrorCode.Redis_Fail_SetToken;
        }

        return ErrorCode.None;
    }

    async Task<Tuple<ErrorCode, Int64>> CreateDefaultUserData(string playerId, string nickname, List<SqlKata.Query> queries)
    {
        try
        {
            // 유저 데이터 생성
            var userId = await _gameDb.InsertGetIdDefaultUserData(playerId, nickname);

            // 성공했다면 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_basicinfo").Where("UserId", userId).AsDelete();
            queries.Add(query);

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, userId);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_CreateDefaultUserData_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerId, Nickname = nickname }, "Failed");

            return new Tuple<ErrorCode, Int64>(errorCode, 0);
        }
    }

    async Task<ErrorCode> CreateDefaultAttendanceData(Int64 userId, List<SqlKata.Query> queries)
    {
        try
        {
            // 출석 데이터 생성
            var insertedRow = await _gameDb.InsertDefaultAttendanceData(userId);
            if (!ValidateAffectedRow(insertedRow, 1))
            {
                return ErrorCode.AuthCheckService_CreateDefaultAttendanceData_AffectedRowOutOfRange;
            }

            // 성공했다면 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_attendance").Where("UserId", userId).AsDelete();
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_CreateDefaultAttendanceData_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId }, "Failed");

            return errorCode;
        }
    }

    async Task<ErrorCode> CreateDefaultFarmData(Int64 userId, List<SqlKata.Query> queries)
    {
        try
        {
            // 농장 기본 데이터 생성
            var insertedRow = await _gameDb.InsertDefaultFarmData(userId);
            if (!ValidateAffectedRow(insertedRow, 1))
            {
                return ErrorCode.AuthCheckService_CreateDefaultFarmData_AffectedRowOutOfRange;
            }

            // 성공했다면 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_info").Where("UserId", userId).AsDelete();
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_CreateDefaultFarmData_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId }, "Failed");

            return errorCode;
        }
    }

    async Task<ErrorCode> CreateDefaultItems(Int64 userId, List<SqlKata.Query> queries)
    {
        try
        {
            // 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_item").Where("OwnerId", userId).AsDelete();
            queries.Add(query);

            // 기본 아이템 Insert
            var insertedRow = await _gameDb.InsertDefaultItems(userId);
            if (!ValidateAffectedRow(insertedRow, _masterDb!._defaultFarmItemList!.Count))
            {
                return ErrorCode.AuthCheckService_CreateDefaultItems_AffectedRowOutOfRange;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_CreateDefaultItems_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId }, "Failed");

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