using System.Text.Json;
using ZLogger;

public class AuthCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<AuthCheckMiddleware> _logger;
    readonly IRedisDb _redisDb;
    HashSet<string> _exceptPath = new HashSet<string>()
    {
        "/api/account/login", "/api/account/create",
    };

    public AuthCheckMiddleware(RequestDelegate next, ILogger<AuthCheckMiddleware> logger, IRedisDb redisDb)
    {
        _next = next;
        _logger = logger;
        _redisDb = redisDb;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path;

        // 해당 요청이 그냥 넘겨야 하는 Path인지 체크
        if (IsExceptPath(path))
        {
            await _next(context);
            return;
        }
        else
        {
            // Enable Buffering
            context.Request.EnableBuffering();
            var bodyStream = new StreamReader(context.Request.Body);
            var rawBody = await bodyStream.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // JSON 포맷 유효성 검사 : PlayerID값과 authToken값을 가져옴
            if (!IsValidFormattedJSON(rawBody, out string playerID, out string authToken, out Int64 userId))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], StatusCode = 400 }, "IsValidFormattedJSON");
                context.Response.StatusCode = 400;
                return;
            }

            // 토큰 유효성 검사
            if (!await IsValidToken(playerID, authToken, userId))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], StatusCode = 401 }, "IsValidToken");
                context.Response.StatusCode = 401;
                return;
            }

            // 중복 요청 검사
            if (await IsOverlappedRequest(authToken, path))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], StatusCode = 429 }, "IsOverlappedRequest");
                context.Response.StatusCode = 429;
                return;
            }

            await _next(context);

            // 중복 요청 방지 해제
            await _redisDb.ReleaseRequest(authToken, path);
        }
    }

    async Task<bool> IsOverlappedRequest(string authToken, string path)
    {
        if (!await _redisDb.AcquireRequest(authToken, path))
        {
            return true;
        }

        return false;
    }

    async Task<bool> IsValidToken(string authID, string authToken, Int64 userId)
    {
        if (authID == string.Empty || authToken == string.Empty || userId == 0)
        {
            return false;
        }

        var memoryToken = await _redisDb.GetAsync(authID);
        if (memoryToken == null)
        {
            return false;
        }

        var memoryUserId = await _redisDb.GetAsync(authToken);
        if (memoryUserId == null || memoryUserId != userId.ToString())
        {
            return false;
        }

        if (!authToken.Equals(memoryToken))
        {
            return false;
        }

        return true;
    }
    bool IsValidFormattedJSON(string rawBody, out string playerID, out string authToken, out Int64 userId)
    {
        playerID = string.Empty;
        authToken = string.Empty;
        userId = 0;

        try
        {
            JsonDocument doc = JsonDocument.Parse(rawBody);
            var playerIDString = doc.RootElement.GetProperty("PlayerID").GetString();
            var authTokenString = doc.RootElement.GetProperty("AuthToken").GetString();
            userId = doc.RootElement.GetProperty("UserID").GetInt64();

            if (playerIDString == null || authTokenString == null || userId == 0)
            {
                return false;
            }

            playerID = playerIDString;
            authToken = authTokenString;

            return true;
        }
        catch
        {
            return false;
        }
    }

    bool IsExceptPath(PathString path)
    {
        if (_exceptPath.Contains(path))
        {
            return true;
        }
        return false;
    }
}