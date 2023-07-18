using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
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
            return new ResCreateDTO() { Result = ErrorCode.AuthCheckFail };
        }

        // GameDB에 해당 PlayerID로 된 계정 데이터가 존재하는지 확인
        if (0 != await _gameDb.GetUserIdByPlayerId(request.PlayerID))
        {
            return new ResCreateDTO() { Result = ErrorCode.UserAlreadyExists };
        }

        // GameDB에 기본 게임 데이터 생성
        var errorCode = await _gameDb.CreateDefaultData(request.PlayerID, request.Nickname);

        LogResult(errorCode, "CreateDefaultData", request.PlayerID, request.AuthToken);
        return new ResCreateDTO() { Result = errorCode };
    }

    [HttpPost("login")]
    public async Task<ResLoginDTO> Login(ReqLoginDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheck(request.PlayerID, request.AuthToken))
        {
            return new ResLoginDTO() { Result = ErrorCode.AuthCheckFail };
        }

        // GameDB에 해당 PlayerID로 된 계정 데이터가 존재하는지 확인
        var userId = await _gameDb.GetUserIdByPlayerId(request.PlayerID);
        if (0 == await _gameDb.GetUserIdByPlayerId(request.PlayerID))
        {
            return new ResLoginDTO() { Result = ErrorCode.UserNotExists };
        }

        // 토큰 생성 및 Redis에 세팅
        var token = Security.CreateAuthToken();
        if (ErrorCode.None != await SetTokenOnRedis(userId, token))
        {
            return new ResLoginDTO() { Result = ErrorCode.TokenSettingFailed };
        }

        // 게임 데이터 로드
        var defaultDataResult = await _gameDb.GetDefaultDataByUserId(userId);
        if (defaultDataResult.Item1 != ErrorCode.None)
        {
            return new ResLoginDTO() { Result = defaultDataResult.Item1 };
        }

        // 최종 로그인 시각 갱신
        if (!await _gameDb.UpdateLastLoginAt(userId))
        {
            _logger.ZLogDebugWithPayload(new { Type = "UpdateLastLoginAt",
                ErrorCode = ErrorCode.LastLoginUpdateFailed, PlayerID = request.PlayerID,
                UserID = userId, AuthToken = request.AuthToken }, "Failed");
        }

        _logger.ZLogInformationWithPayload(new { Type = "Login",
            PlayerID = request.PlayerID, UserID = userId,
            AuthToken = request.AuthToken }, "Statistic");

        return new ResLoginDTO() { Result = ErrorCode.None, DefaultData = defaultDataResult.Item2, AuthToken = token };
    }

    //[HttpPost("logout")]
    //public async Task<ResLogoutDTO> Logout(ReqLogoutDTO request)
    //{
    //    if (!await _redisDb.DeleteSessionDataAsync(request.AuthID, request.AuthToken, request.UserID))
    //    {
    //        return new ResLogoutDTO() { Result = ErrorCode.LogoutFail };
    //    }

    //    return new ResLogoutDTO() { Result = ErrorCode.None };
    //}

    //[HttpPut("nickname")]
    //public async Task<ResChangeNicknameDTO> ChangeNickname(ReqChangeNicknameDTO request)
    //{
    //    // 유저 닉네임 변경
    //    if (!(await _gameDb.TryChangeNickname(request.AuthID, request.Nickname)))
    //    {
    //        // 중복 닉네임
    //        return new ResChangeNicknameDTO() { Result = ErrorCode.DuplicateNickname };
    //    }

    //    // 변경 성공
    //    return new ResChangeNicknameDTO() { Result = ErrorCode.None };
    //}

    void LogResult(ErrorCode errorCode, string method, string playerId, string authToken)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogDebugWithPayload(new { Type = method, ErrorCode = errorCode, PlayerID = playerId, AuthToken = authToken }, "Failed");
        } else
        {
            _logger.ZLogInformationWithPayload(new { Type = method, PlayerID = playerId, AuthToken = authToken }, "Statistic");
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
                _logger.ZLogDebugWithPayload(new { Type = "AuthCheck", ErrorCode = ErrorCode.AuthCheckFail, PlayerID = playerID, AuthToken = authToken, StatusCode = hiveResponse == null? 0 : hiveResponse.StatusCode }, "Failed");
                return false;
            }

            // 인증 정보(ErrorCode) 체크
            var authResult = await hiveResponse.Content.ReadFromJsonAsync<ErrorCodeDTO>();
            if (authResult == null || authResult.Result != ErrorCode.None)
            {
                _logger.ZLogDebugWithPayload(new { Type = "AuthCheck", ErrorCode = ErrorCode.AuthCheckFail, PlayerID = playerID, AuthToken = authToken}, "Failed");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogDebugWithPayload(new { Type = "AuthCheck", ErrorCode = ErrorCode.AuthCheckFail, PlayerID = playerID, AuthToken = authToken, Exception = ex.GetType().ToString() }, "Failed");
            return false;
        }
    }

    async Task<ErrorCode> SetTokenOnRedis(Int64 userId, string sessionToken)
    {
        // 같은 키의 토큰이 있어도 무조건 Overwrite하여 기존 토큰을 무효화
        if (!await _redisDb.SetAsync(userId, sessionToken, TimeSpan.FromDays(7)))
        {
            return ErrorCode.TokenSettingFailed;
        }

        return ErrorCode.None;
    }

    public class Security
    {
        private const String AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static String CreateAuthToken()
        {
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return new String(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
        }
    }
}