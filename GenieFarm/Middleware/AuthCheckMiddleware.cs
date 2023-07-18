using System.Text.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using ZLogger;

public class AuthCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<AuthCheckMiddleware> _logger;
    readonly IMasterDb _masterDb;
    readonly IRedisDb _redisDb;
    // Redis 토큰 저장여부 검사를 제외하는 Path들
    readonly HashSet<string> _tokenExceptPath = new HashSet<string>()
    {
        "/api/account/login", "/api/account/create",
    };

    public AuthCheckMiddleware(RequestDelegate next, ILogger<AuthCheckMiddleware> logger, IRedisDb redisDb, IMasterDb masterDb)
    {
        _next = next;
        _logger = logger;
        _redisDb = redisDb;
        _masterDb = masterDb;
    }

    public async Task Invoke(HttpContext context)
    {
        // Redis에 토큰을 저장해야 하는 Path인지 확인
        var path = context.Request.Path;
        var exceptRedisCheck = IsExceptPath(path);

        // 버퍼링 Enable 및 Request Body 가져오기
        var rawBody = await EnableBuffering(context);
        if (rawBody == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        // JSON 포맷 유효성 검사 : PlayerID값과 authToken값을 가져옴
        if (!IsValidFormattedJSON(rawBody, out string playerID, out string authToken, out string appVersion, out string masterDataVersion, out Int64 userId, exceptRedisCheck))
        {
            context.Response.StatusCode = 400;
            return;
        }

        // 앱 버전, 게임 데이터 버전 확인
        if (!VersionCheck(appVersion, masterDataVersion))
        {
            context.Response.StatusCode = 426;
            return;
        }

        // Redis에 토큰이 저장되어있어야 하는 Path
        if (!exceptRedisCheck)
        {
            // 토큰 유효성 검사
            if (!await IsValidToken(playerID, authToken, userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // 중복 요청 검사
            if (await IsOverlappedRequest(authToken, path))
            {
                context.Response.StatusCode = 429;
                return;
            }
        }

        await _next(context);

        // 중복 요청 방지 해제
        if (!exceptRedisCheck)
        {
            await _redisDb.ReleaseRequest(authToken, path);
        }
    }
    async Task<string?> EnableBuffering(HttpContext context)
    {
        try
        {
            context.Request.EnableBuffering();
            var bodyStream = new StreamReader(context.Request.Body);
            var rawBody = await bodyStream.ReadToEndAsync();
            context.Request.Body.Position = 0;

            return rawBody;
        }
        catch
        {
            return null;
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

    async Task<bool> IsValidToken(string playerId, string authToken, Int64 userId)
    {
        if (playerId == string.Empty || authToken == string.Empty || userId == 0)
        {
            return false;
        }

        var memoryToken = await _redisDb.GetAsync(playerId);
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

    bool IsValidFormattedJSON(string rawBody, out string playerID, out string authToken, out string appVersion, out string masterDataVersion, out Int64 userId, bool exceptRedisCheck)
    {
        playerID = string.Empty;
        authToken = string.Empty;
        appVersion = string.Empty;
        masterDataVersion = string.Empty;
        userId = 0;

        try
        {
            JsonDocument doc = JsonDocument.Parse(rawBody);
            var playerIDString = doc.RootElement.GetProperty("PlayerID").GetString();
            var authTokenString = doc.RootElement.GetProperty("AuthToken").GetString();
            var appVersionString = doc.RootElement.GetProperty("AppVersion").GetString();
            var masterDataVersionString = doc.RootElement.GetProperty("MasterDataVersion").GetString();

            if (!exceptRedisCheck)
            {
                userId = doc.RootElement.GetProperty("UserID").GetInt64();
            }

            if (playerIDString == null || authTokenString == null || appVersionString == null || masterDataVersionString == null)
            {
                return false;
            }

            playerID = playerIDString;
            authToken = authTokenString;
            appVersion = appVersionString;
            masterDataVersion = masterDataVersionString;

            return true;
        }
        catch
        {
            return false;
        }
    }

    bool IsExceptPath(PathString path)
    {
        if (_tokenExceptPath.Contains(path))
        {
            return true;
        }
        return false;
    }

    bool VersionCheck(string appVersion, string masterDataVersion)
    {
        if (!(appVersion.Equals(_masterDb._version!.AppVersion)))
        {
            return false;
        }

        if (!(masterDataVersion.Equals(_masterDb._version!.MasterDataVersion)))
        {
            return false;
        }

        return true;
    }
}