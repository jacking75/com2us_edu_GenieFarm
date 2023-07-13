using System.Text.Json;

public class AuthCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthCheckMiddleware> _logger;
    private readonly IRedisDb _redisDb;
    private HashSet<String> _exceptPath = new HashSet<String>()
    {
        "/api/user/login", "/api/user/logout", "/api/user/register", "/api/user/nickname"
    };

    public AuthCheckMiddleware(RequestDelegate next, ILogger<AuthCheckMiddleware> logger, IRedisDb redisDb)
    {
        _next = next;
        _logger = logger;
        _redisDb = redisDb;
        _logger.LogInformation("[AuthCheckMiddleware] Instance Created");
    }
    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path;
        _logger.LogInformation("[AuthCheckMiddleware.Invoke] Current Request Path is {}", path);


        // 해당 요청이 그냥 넘겨야 하는 Path인지 체크한다.
        if (IsExceptPath(path))
        {
            await _next(context);
            return;
        }
        else
        {
            // Body를 여러 번 읽기 위해 Buffering 설정
            context.Request.EnableBuffering();

            // StreamReader 생성
            var bodyStream = new StreamReader(context.Request.Body);
            var rawBody = await bodyStream.ReadToEndAsync();
            _logger.LogInformation("[AuthCheckMiddleware.Invoke] Raw Body : {}", rawBody);
            // Stream Position 리셋 (다음에 다시 읽을 수 있도록)
            context.Request.Body.Position = 0;

            // JSON 포맷 유효성 검사
            // 포맷 검사를 하면서, AuthID값과 authToken값을 가져옴
            if (!IsValidFormat(rawBody, out String? authID, out String? authToken, out Int64 userId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            // 토큰 유효성 검사
            if (!await IsValidToken(authID, authToken, userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // 중복 요청 검사
            if (await IsOverlapRequest(authToken, path))
            {
                _logger.LogInformation("[AuthCheckMiddleware.Invoke] Overlapped Request");
                context.Response.StatusCode = 429;
                return;
            }

            await _next(context);
            await _redisDb.ReleaseRequest(authToken, path);
        }
    }

    private async Task<bool> IsOverlapRequest(String? authToken, String? path)
    {
        if (!await _redisDb.AcquireRequest(authToken, path))
        {
            return true;
        }

        return false;
    }

    private async Task<bool> IsValidToken(String? authID, String? authToken, Int64 userId)
    {
        _logger.LogInformation("[AuthCheckMiddleware.IsValidToken] AuthID : {}, AuthToken : {}", authID, authToken);
        if (authID == null || authToken == null)
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

        _logger.LogInformation("[AuthCheckMiddleware.IsValidToken] Token From Memory : {}", memoryToken);

        if (!authToken.Equals(memoryToken))
        {
            return false;
        }

        return true;
    }

    private bool IsValidFormat(String rawBody, out String? authID, out String? authToken, out Int64 userId)
    {
        try
        {
            JsonDocument doc = JsonDocument.Parse(rawBody);
            authID = doc.RootElement.GetProperty("AuthID").GetString();
            authToken = doc.RootElement.GetProperty("AuthToken").GetString();
            userId = doc.RootElement.GetProperty("UserID").GetInt64();

            if (authID == null || authToken == null || userId == 0)
            {
                return false;
            }

            return true;
        }
        catch
        {
            _logger.LogInformation("[AuthCheckMiddleware.InValidFormat] Request Body is Not Valid JSON");
            authID = null;
            authToken = null;
            userId = 0;

            return false;
        }
    }

    private bool IsExceptPath(PathString path)
    {
        if (_exceptPath.Contains(path))
        {
            return true;
        }
        return false;
    }
}