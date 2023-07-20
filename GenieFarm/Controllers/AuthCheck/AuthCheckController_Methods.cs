using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

public partial class AuthCheckController : ControllerBase
{
    ILogger<AuthCheckController> _logger;
    IGameDb _gameDb;
    IRedisDb _redisDb;
    string _hiveServerUrl;


    public AuthCheckController(ILogger<AuthCheckController> logger, IGameDb gameDb, IRedisDb redisDb, IConfiguration configuration)
    {
        _logger = logger;
        _gameDb = gameDb;
        _redisDb = redisDb;
        _hiveServerUrl = configuration.GetSection("HiveServer")["Address"]! + "/authcheck";
    }

    void LogResult(ErrorCode errorCode, string method, string playerId, string authToken)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create((UInt16)errorCode, method),
                                         new { PlayerID = playerId, AuthToken = authToken }, "Failed");
        }
        else
        {
            _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method),
                                               new { PlayerID = playerId, AuthToken = authToken }, "Statistic");
        }
    }

    async Task<bool> AuthCheckToHive(string playerID, string authToken)
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
            if (authResult == null || authResult.Result != ErrorCode.None)
            {
                var errorCode = ErrorCode.Hive_Fail_AuthCheck;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new { PlayerID = playerID, AuthToken = authToken }, "Failed");

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
        private const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string CreateAuthToken()
        {
            // 랜덤하게 토큰을 생성
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
        }
    }
}
