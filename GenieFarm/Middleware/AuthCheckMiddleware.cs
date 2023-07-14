using System.Text.Json;
using ZLogger;

public class AuthCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<AuthCheckMiddleware> _logger;
    readonly IRedisDb _redisDb;
    HashSet<String> _exceptPath = new HashSet<String>()
    {
        "/api/account/login", "/api/account/logout", "/api/account/register", "/api/account/nickname"
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
            // Request Body 로드
            if (!GetRawBody(context, out var rawBody))
            {
                context.Response.StatusCode = 400;
                return;
            }

            // JSON 포맷 유효성 검사 : AuthID값과 authToken값을 가져옴
            if (!IsValidFormattedJSON(rawBody, out string authID, out string authToken, out Int64 userId))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], IsRequest = false, StatusCode = 400 }, "IsValidFormattedJSON");
                context.Response.StatusCode = 400;
                return;
            }

            // 토큰 유효성 검사
            if (!await IsValidToken(authID, authToken, userId))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], IsRequest = false, StatusCode = 401 }, "IsValidToken");
                context.Response.StatusCode = 401;
                return;
            }

            // 중복 요청 검사
            if (await IsOverlappedRequest(authToken, path))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = context.Items["RequestId"], IsRequest = false, StatusCode = 429 }, "IsOverlappedRequest");
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

    bool GetRawBody(HttpContext context, out string rawBody)
    {
        rawBody = string.Empty;

        if (context.Items["RawBody"] != null && context.Items["RawBody"] is string)
        {
            var rawString = context.Items["RawBody"] as string;

            if (rawString == null)
            {
                return false;
            }

            rawBody = rawString;
            return true;
        }

        return false;
    }

    bool IsValidFormattedJSON(string rawBody, out string authID, out string authToken, out Int64 userId)
    {
        authID = string.Empty;
        authToken = string.Empty;
        userId = 0;

        try
        {
            JsonDocument doc = JsonDocument.Parse(rawBody);
            var authIDString = doc.RootElement.GetProperty("AuthID").GetString();
            var authTokenString = doc.RootElement.GetProperty("AuthToken").GetString();
            userId = doc.RootElement.GetProperty("UserID").GetInt64();

            if (authIDString == null || authTokenString == null || userId == 0)
            {
                return false;
            }

            authID = authIDString;
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