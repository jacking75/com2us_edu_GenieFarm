using System.Configuration;
using System.Net;
using Org.BouncyCastle.Asn1.Ocsp;
using SqlKata;
using SqlKata.Execution;
using ZLogger;

/// <summary>
/// 로그인, 계정 생성 등 AuthCheck와 관련된 비즈니스 로직을 처리하고 <br/>
/// DB Operation Call을 수행하는 서비스 클래스
/// </summary>
public class AuthCheckService : IAuthCheckService
{
    readonly ILogger<AuthCheckService> _logger;
    readonly IGameDb _gameDb;
    readonly IMasterDb _masterDb;
    readonly IRedisDb _redisDb;
    string _hiveServerUrl;

    public AuthCheckService(ILogger<AuthCheckService> logger, IGameDb gameDb, IMasterDb masterDb, IRedisDb redisDb, IConfiguration configuration)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
        _redisDb = redisDb;
        _hiveServerUrl = configuration.GetSection("HiveServer")["Address"]! + "/authcheck";
    }

    /// <summary>
    /// Hive 서버에 인증을 요청합니다.
    /// </summary>
    public async Task<bool> AuthCheckToHive(string playerID, string authToken)
    {
        try
        {
            // 인증 요청
            HttpClient client = new();
            var hiveResponse = await client.PostAsJsonAsync(_hiveServerUrl,
                                                            new { AuthID = playerID,
                                                                  AuthToken = authToken });

            // 응답 체크
            if (ValidateHiveResponse(hiveResponse))
            {
                var statusCode = hiveResponse == null ? 0 : hiveResponse.StatusCode;
                var errorCode = ErrorCode.Hive_Fail_InvalidResponse;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new
                                             {
                                                 PlayerID = playerID,
                                                 AuthToken = authToken,
                                                 StatusCode = statusCode
                                             }, "Failed");

                return false;
            }

            // 인증 정보(ErrorCode) 체크
            var authResult = await hiveResponse.Content.ReadFromJsonAsync<ErrorCodeDTO>();
            return ValidateHiveAuthErrorCode(authResult);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Hive_Fail_AuthCheckException;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerID, AuthToken = authToken }, "Failed");

            return false;
        }
    }

    /// <summary>
    /// PlayerID로 된 계정이 DB에 존재하는지 확인하고, <br/>
    /// 존재한다면 ErrorCode.None을 리턴합니다.
    /// </summary>
    public async Task<ErrorCode> CheckPlayerExists(string playerId)
    {
        var userId = await _gameDb.GetUserIdByPlayerId(playerId);
        if (userId == 0)
        {
            return ErrorCode.AuthCheckService_CheckPlayerExists_NotExists;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// PlayerID와 Nickname으로 기본 게임 데이터를 생성합니다. <br/>
    /// 기본 유저 데이터 생성, 출석 데이터 생성, <br/>
    /// 농장 기본 데이터 생성, 기본 아이템 Insert를 수행합니다.
    /// </summary>
    public async Task<ErrorCode> CreateDefaultGameData(string playerId, string nickname)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 유저 데이터 생성
        (var defaultUserDataResult, var userId) = await CreateDefaultUserData(playerId, nickname, rollbackQueries);
        if (!Successed(defaultUserDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(defaultUserDataResult),
                                         new { PlayerID = playerId, Nickname = nickname },
                                         "Failed");

            return ErrorCode.AuthCheckService_CreateDefaultGameData_DuplicatedNickname;
        }

        // 출석 데이터 생성
        var attendanceDataResult = await CreateDefaultAttendanceData(userId, rollbackQueries);
        if (!Successed(attendanceDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(attendanceDataResult),
                                         new { UserID = userId }, "Failed");

            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_AttendData;
        }

        // 농장 기본 데이터 생성
        var farmDataResult = await CreateDefaultFarmData(userId, rollbackQueries);
        if (!Successed(farmDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(farmDataResult),
                                         new { UserID = userId }, "Failed");

            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_FarmData;
        }

        // 기본 아이템 Insert
        var defaultItemResult = await CreateDefaultItems(userId, rollbackQueries);
        if (!Successed(defaultItemResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(defaultItemResult),
                                        new { UserID = userId }, "Failed");

            await Rollback(attendanceDataResult, rollbackQueries);

            return ErrorCode.AuthCheckService_CreateDefaultGameData_Items;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// Hive PlayerID로 기본 게임 데이터를 로드합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(string playerId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await _gameDb.GetDefaultUserDataByPlayerId(playerId);
        if (result.UserData == null)
        {
            return new (ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_UserData, null);
        }

        // 출석 정보 로드
        var userId = result.UserData.UserId;
        result.AttendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (result.AttendData == null)
        {
            return new (ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_AttendData, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await _gameDb.GetDefaultFarmDataByUserId(userId);
        if (result.FarmInfoData == null)
        {
            return new (ErrorCode.AuthCheckService_GetDefaultGameDataByPlayerId_FarmData, null);
        }

        return new (ErrorCode.None, result);
    }

    /// <summary>
    /// 최종 로그인 시각을 갱신합니다.<br/>
    /// 로그인 시에 사용됩니다.
    /// </summary>
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

    /// <summary>
    /// Redis에 토큰을 저장합니다.
    /// </summary>
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

    /// <summary>
    /// user_basicinfo 테이블에 기본 데이터를 생성하고, <br/>
    /// UserID(Primary Key)값을 가져옵니다.
    /// </summary>
    async Task<Tuple<ErrorCode, Int64>> CreateDefaultUserData(string playerId, string nickname, List<SqlKata.Query> queries)
    {
        try
        {
            // 유저 데이터 생성
            var userId = await _gameDb.InsertGetIdDefaultUserData(playerId, nickname);

            // 성공했다면 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_basicinfo").Where("UserId", userId).AsDelete();
            queries.Add(query);

            return new (ErrorCode.None, userId);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuthCheckService_CreateDefaultUserData_Fail;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerId, Nickname = nickname }, "Failed");

            return new (errorCode, 0);
        }
    }

    /// <summary>
    /// user_attendance 테이블에 기본 데이터를 생성합니다.
    /// </summary>
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

    /// <summary>
    /// farm_info 테이블에 농장 기본 데이터를 생성합니다.
    /// </summary>
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

    /// <summary>
    /// farm_item 테이블에 기본 지급 아이템들을 Insert합니다.
    /// </summary>
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
    /// Redis에 있는 토큰을 삭제합니다.
    /// </summary>
    public async Task<ErrorCode> DeleteTokenOnRedis(Int64 userId)
    {
        if (false == await _redisDb.DeleteAsync(userId.ToString()))
        {
            var errorCode = ErrorCode.Redis_Fail_DeleteToken;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { UserID = userId }, "Failed");

            return errorCode;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// Hive 서버에서 온 응답이 유효한지 null 체크와 StatusCode 체크를 합니다.
    /// </summary>
    bool ValidateHiveResponse(HttpResponseMessage? response)
    {
        if (response == null || response.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Hive 서버에서 온 에러코드가 ErrorCode.None인지 체크하고 맞다면 true를 반환합니다.
    /// </summary>
    bool ValidateHiveAuthErrorCode(ErrorCodeDTO? authResult)
    {
        if (authResult == null || authResult.Result != ErrorCode.None)
        {
            return false;
        }

        return true;
    }
}