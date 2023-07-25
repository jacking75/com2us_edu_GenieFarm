using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Operators;
using ZLogger;

[ApiController]
[Route("api/auth")]

public partial class AuthCheckController : ControllerBase
{
    ILogger<AuthCheckController> _logger;
    IAuthCheckService _authCheckService;
    IGameDb _gameDb;
    IRedisDb _redisDb;
    string _hiveServerUrl;

    public AuthCheckController(ILogger<AuthCheckController> logger, IGameDb gameDb, IRedisDb redisDb, IAuthCheckService authCheckService, IConfiguration configuration)
    {
        _logger = logger;
        _gameDb = gameDb;
        _redisDb = redisDb;
        _authCheckService = authCheckService;
        _hiveServerUrl = configuration.GetSection("HiveServer")["Address"]! + "/authcheck";
    }

    /// <summary>
    /// 계정 생성 API <br/>
    /// 하이브 서버 인증 후, 계정 데이터가 없다면 새로 생성합니다.
    /// </summary>
    [HttpPost("create")]
    public async Task<ResCreateDTO> Create(ReqCreateDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheckToHive(request.PlayerID, request.AuthToken))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.Hive_Fail_AuthCheck),
                                         new { PlayerID = request.PlayerID,
                                               AuthToken = request.AuthToken }, "Failed");

            return new ResCreateDTO() { Result = ErrorCode.Login_Fail_HiveAuthCheck };
        }

        // GameDB에 해당 PlayerID로 된 계정 데이터가 존재하는지 확인
        if (ErrorCode.None == await _authCheckService.CheckPlayerExists(request.PlayerID))
        {
            return new ResCreateDTO() { Result = ErrorCode.Create_Fail_UserAlreadyExists };
        }

        // GameDB에 기본 게임 데이터 생성
        var errorCode = await _authCheckService.CreateDefaultGameData(request.PlayerID, request.Nickname);
        if (!Successed(errorCode))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { PlayerID = request.PlayerID,
                                               Nickname = request.Nickname }, "Failed");
            return new ResCreateDTO() { Result = ErrorCode.Create_Fail_CreateDefaultDataFailed };
        }

        LogInfoOnSuccess("Create", new { PlayerID = request.PlayerID });
        return new ResCreateDTO() { Result = errorCode };
    }

    /// <summary>
    /// 로그인 API <br/>
    /// 하이브 서버 인증 후, 기본 게임 데이터를 로드 및 마지막 로그인 시각을 갱신합니다. <br/>
    /// 토큰을 발급해 Redis에 저장합니다.
    /// </summary>
    [HttpPost("login")]
    public async Task<ResLoginDTO> Login(ReqLoginDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheckToHive(request.PlayerID, request.AuthToken))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.Hive_Fail_AuthCheckOnLogin),
                                         new { PlayerID = request.PlayerID,
                                               AuthToken = request.AuthToken }, "Failed");

            return new ResLoginDTO() { Result = ErrorCode.Login_Fail_HiveAuthCheck };
        }

        // 게임 데이터 로드
        (var defaultDataResult, var defaultData) = await _authCheckService.GetDefaultGameData(request.PlayerID);
        if (!Successed(defaultDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(defaultDataResult),
                                         new { PlayerID = request.PlayerID }, "Failed");

            return new ResLoginDTO() { Result = ErrorCode.Login_Fail_UserDataNotExists };
        }

        // 최종 로그인 시각 갱신
        var userId = defaultData!.UserData!.UserId;
        var lastLoginUpdateResult = await _authCheckService.UpdateLastLoginAt(userId);
        if (!Successed(lastLoginUpdateResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(lastLoginUpdateResult),
                                         new { UserID = userId }, "Failed");

            return new ResLoginDTO() { Result = ErrorCode.Login_Fail_UpdateLastLogin };
        }

        // 토큰 생성 및 Redis에 세팅
        var token = Security.CreateAuthToken();
        var setTokenResult = await _authCheckService.SetTokenOnRedis(userId, token);
        if (!Successed(setTokenResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(setTokenResult),
                                         new { UserID = userId, AuthToken = token }, "Failed");

            return new ResLoginDTO() { Result = ErrorCode.Login_Fail_TokenSetting };
        }

        LogInfoOnSuccess("Login", new { PlayerID = request.PlayerID });
        return new ResLoginDTO() { Result = ErrorCode.None, DefaultData = defaultData, AuthToken = token };
    }

    /// <summary>
    /// 로그아웃 API <br/>
    /// Redis에서 토큰을 삭제합니다.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ResLogoutDTO> Logout(ReqLogoutDTO request)
    {
        // Redis에서 토큰 삭제
        var deleteResult = await _authCheckService.DeleteTokenOnRedis(request.UserID);
        if (!Successed(deleteResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(deleteResult),
                                         new { UserID = request.UserID }, "Failed");

            return new ResLogoutDTO() { Result = ErrorCode.Logout_Fail_DeleteToken };
        }

        LogInfoOnSuccess("Logout", new { PlayerID = request.UserID });
        return new ResLogoutDTO() { Result = ErrorCode.None };
    }
}