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
    [HttpPost("create")]
    public async Task<ResCreateDTO> Create(ReqCreateDTO request)
    {
        // 하이브 서버에 인증 요청
        if (!await AuthCheckToHive(request.PlayerID, request.AuthToken))
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
        if (!await AuthCheckToHive(request.PlayerID, request.AuthToken))
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

    [HttpPost("logout")]
    public async Task<ResLogoutDTO> Logout(ReqLogoutDTO request)
    {
        // Redis에서 토큰 삭제
        if (false == await _redisDb.DeleteAsync(request.UserID.ToString()))
        {
            return new ResLogoutDTO() { Result = ErrorCode.Redis_Fail_DeleteToken };
        }

        LogResult(ErrorCode.None, "Logout", request.UserID, request.AuthToken);
        return new ResLogoutDTO() { Result = ErrorCode.None };
    }
}