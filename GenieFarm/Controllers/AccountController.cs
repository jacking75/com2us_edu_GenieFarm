using System.Net;
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

        // GameDB에 해당 PlayerID로 된 데이터가 존재하는지 확인
        if (await _gameDb.CheckPlayerIdExists(request.PlayerID))
        {
            return new ResCreateDTO() { Result = ErrorCode.UserAlreadyExists };
        }

        // GameDB에 기본 게임 데이터 생성
        var errorCode = await _gameDb.CreateDefaultData(request.PlayerID, request.Nickname);

        LogResult(errorCode, "CreateDefaultData", request.PlayerID, request.AuthToken);
        return new ResCreateDTO() { Result = errorCode };
    }

    //[HttpPost("login")]
    //public async Task<ResLoginDTO> Login(ReqLoginDTO request)
    //{
    //    // 하이브 서버에 인증 요청
    //    if (!await AuthCheck(request.PlayerID, request.AuthToken))
    //    {
    //        return new ResLoginDTO() { Result = ErrorCode.AuthCheckFail };
    //    }

    //    // GameDB에 기본 게임 데이터가 있는지 체크
    //    var accountData = await _gameDb.GetDefaultDataByPlayerId(request.PlayerID);
    //    if (accountData == null)
    //    {
    //        return new ResLoginDTO() { Result = ErrorCode.UserNotExists };
    //    }

    //    // Redis 토큰 세팅
    //    if (ErrorCode.None != await SetTokenOnRedis(request.PlayerID, request.AuthToken, accountData.UserId))
    //    {
    //        return new ResLoginDTO() { Result = ErrorCode.SessionSettingFail };
    //    }

    //    // 최종 로그인 시각 변경
    //    await _gameDb.UpdateLastLoginAt(accountData.UserId);

    //    // 유저 데이터 반환
    //    return new ResLoginDTO() { Result = ErrorCode.None, UserData = accountData };
    //}

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
                return false;
            }

            // 인증 정보(ErrorCode) 체크
            var authResult = await hiveResponse.Content.ReadFromJsonAsync<ErrorCodeDTO>();
            if (authResult == null || authResult.Result != ErrorCode.None)
            {
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


    //async Task<ErrorCode> SetTokenOnRedis(string authId, string authToken, Int64 userId)
    //{
    //    // 같은 키의 토큰이 있어도 무조건 Overwrite하여 기존 토큰을 무효화
    //    if (!await _redisDb.SetAsync(authId, authToken, TimeSpan.FromDays(7)))
    //    {
    //        return ErrorCode.SessionSettingFail;
    //    }
    //    if (!await _redisDb.SetAsync(authToken, userId, TimeSpan.FromDays(7)))
    //    {
    //        return ErrorCode.SessionSettingFail;
    //    }

    //    return ErrorCode.None;
    //}
}