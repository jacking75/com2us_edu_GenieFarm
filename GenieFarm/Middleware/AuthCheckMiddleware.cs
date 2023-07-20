using System.Text.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using ZLogger;

public class AuthCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<AuthCheckMiddleware> _logger;
    readonly IRedisDb _redisDb;

    public AuthCheckMiddleware(RequestDelegate next, ILogger<AuthCheckMiddleware> logger, IRedisDb redisDb)
    {
        _next = next;
        _logger = logger;
        _redisDb = redisDb;
    }

    public async Task Invoke(HttpContext context)
    {
        // UserID와 AuthToken값을 헤더에서 가져오기
        if (!GetAuthDataFromHeader(context, out var userId, out var authToken))
        {
            context.Response.StatusCode = 400;
            return;
        }

        // 토큰 유효성 검사
        if (!await ValidateToken(userId, authToken))
        {
            context.Response.StatusCode = 401;
            return;
        }

        // 중복 요청 검사
        var path = context.Request.Path.Value;
        if (await CheckOverlappedRequest(authToken, path!))
        {
            context.Response.StatusCode = 429;
            return;
        }

        await _next(context);

        await _redisDb.ReleaseRequest(authToken, path!);
    }
    
    bool GetAuthDataFromHeader(HttpContext context, out string userId, out string authToken)
    {
        userId = context.Request.Headers["UserID"].ToString();
        authToken = context.Request.Headers["AuthToken"].ToString();

        if (userId == string.Empty || authToken == string.Empty)
        {
            return false;
        }

        return true;
    }

    async Task<bool> CheckOverlappedRequest(string authToken, string path)
    {
        if (!await _redisDb.AcquireRequest(authToken, path))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_RequestOverlapped),
                                         new { AuthToken = authToken, Path = path }, "Failed");

            return true;
        }

        return false;
    }

    async Task<bool> ValidateToken(string userId, string authToken)
    {
        if (userId == string.Empty || authToken == string.Empty)
        {
            return false;
        }

        if (!await _redisDb.CompareMemoryKeyValue(userId.ToString(), authToken))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_TokenNotMatch),
                                         new { UserID = userId, AuthToken = authToken }, "Failed");

            return false;
        }

        return true;
    }
}