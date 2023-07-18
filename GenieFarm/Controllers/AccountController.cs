using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Operators;
using ZLogger;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    ILogger<AccountController> _logger;
    IMasterDb _masterDb;
    IGameDb _gameDb;
    IRedisDb _redisDb;
    Dictionary<string, string> _authUserData = new();
    string _hiveServerUrl;


    public AccountController(ILogger<AccountController> logger, IGameDb gameDb, IRedisDb redisDb, IMasterDb masterDb, IConfiguration configuration)
    {
        _logger = logger;
        _gameDb = gameDb;
        _redisDb = redisDb;
        _masterDb = masterDb;
        _hiveServerUrl = configuration.GetSection("HiveServer")["Address"]! + "/authcheck";
    }

    [HttpPost("create")]
    public async Task<ResCreateDTO> Create(ReqCreateDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheck(request.PlayerID, request.AuthToken))
        {
            return new ResCreateDTO() { Result = ErrorCode.Hive_Fail_AuthCheck };
        }

        // GameDB에 해당 PlayerID로 된 계정 데이터가 존재하는지 확인
        if (0 != await _gameDb.GetUserIdByPlayerId(request.PlayerID))
        {
            return new ResCreateDTO() { Result = ErrorCode.Account_Fail_UserAlreadyExists };
        }

        // GameDB에 기본 게임 데이터 생성
        var errorCode = await _gameDb.CreateDefaultData(request.PlayerID, request.Nickname);

        LogResult(errorCode, "Create", request.PlayerID, request.AuthToken);
        return new ResCreateDTO() { Result = errorCode };
    }

    [HttpPost("login")]
    public async Task<ResLoginDTO> Login(ReqLoginDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheck(request.PlayerID, request.AuthToken))
        {
            return new ResLoginDTO() { Result = ErrorCode.Hive_Fail_AuthCheckOnLogin };
        }

        // 게임 데이터 로드
        (var defaultDataResult, var defaultData) = await _gameDb.GetDefaultDataByPlayerId(request.PlayerID);
        if (defaultDataResult != ErrorCode.None)
        {
            return new ResLoginDTO() { Result = defaultDataResult };
        }

        // 최종 로그인 시각 갱신
        if (!await _gameDb.UpdateLastLoginAt(defaultData!.UserData!.UserId))
        {
            return new ResLoginDTO() { Result = ErrorCode.Account_Fail_UpdateLastLogin };
        }

        // 토큰 생성 및 Redis에 세팅
        var token = Security.CreateAuthToken();
        if (ErrorCode.None != await SetTokenOnRedis(defaultData!.UserData!.UserId, token))
        {
            return new ResLoginDTO() { Result = ErrorCode.Redis_Fail_SetToken };
        }

        LogResult(ErrorCode.None, "Login", request.PlayerID, request.AuthToken);
        return new ResLoginDTO() { Result = ErrorCode.None, DefaultData = defaultData, AuthToken = token };
    }

    void LogResult(ErrorCode errorCode, string method, string playerId, string authToken)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create((UInt16)errorCode, method),
                                         new { PlayerID = playerId, AuthToken = authToken }, "Failed");
        } else
        {
            _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method),
                                               new { PlayerID = playerId, AuthToken = authToken }, "Statistic");
        }
    }

    async Task<bool> AuthCheck(string playerID, string authToken)
    {
        try
        {
            // 인증 요청
            HttpClient client = new();
            var hiveResponse = await client.PostAsJsonAsync(_hiveServerUrl, new { AuthID = playerID, AuthToken = authToken });

            // 응답 체크
            if (hiveResponse == null || hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                var statusCode = hiveResponse == null ? 0 : hiveResponse.StatusCode;

                var errorCode = ErrorCode.Hive_Fail_InvalidResponse;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new { PlayerID = playerID, AuthToken = authToken,
                                                   StatusCode = statusCode }, "Failed");

                return false;
            }

            // 인증 정보(ErrorCode) 체크
            var authResult = await hiveResponse.Content.ReadFromJsonAsync<ErrorCodeDTO>();
            if (authResult == null || authResult.Result != ErrorCode.None)
            {
                var errorCode = ErrorCode.Hive_Fail_AuthCheck;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new { PlayerID = playerID, AuthToken = authToken}, "Failed");

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Hive_Fail_AuthCheckException;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerID, AuthToken = authToken }, "Failed");

            return false;
        }
    }

    async Task<ErrorCode> SetTokenOnRedis(Int64 userId, string sessionToken)
    {
        // 같은 키의 토큰이 있어도 무조건 Overwrite하여 기존 토큰을 무효화
        if (!await _redisDb.SetAsync(userId, sessionToken, TimeSpan.FromHours(10)))
        {
            return ErrorCode.Redis_Fail_SetToken;
        }

        return ErrorCode.None;
    }

    public class Security
    {
        private const String AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static String CreateAuthToken()
        {
            // 랜덤하게 토큰을 생성
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return new String(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
        }
    }
}