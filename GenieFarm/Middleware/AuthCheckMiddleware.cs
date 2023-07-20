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
        "/api/auth/login", "/api/auth/create",
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

        // JSON 포맷 유효성 검사
        if (!ValidateJSONFormat(rawBody, out var doc))
        {
            context.Response.StatusCode = 400;
            return;
        }

        // path에 따라 분기해서 PlayerId, UserId, AuthToken, AppVersion, MasterDataVersion을 가져옴
        if (!GetRequiredField(doc!, out string playerID, out Int64 userId, out string authToken,
                              out string appVersion, out string masterDataVersion, exceptRedisCheck))
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
            if (!await ValidateToken(userId, authToken))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // 중복 요청 검사
            if (await CheckOverlappedRequest(authToken, path))
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

    async Task<bool> CheckOverlappedRequest(string authToken, string path)
    {
        if (!await _redisDb.AcquireRequest(authToken, path))
        {
            return true;
        }

        return false;
    }

    async Task<bool> ValidateToken(Int64 userId, string authToken)
    {
        if (userId == 0 || authToken == string.Empty)
        {
            return false;
        }

        if (!await _redisDb.CompareMemoryKeyValue(userId.ToString(), authToken))
        {
            return false;
        }

        return true;
    }

    bool GetRequiredField(JsonDocument doc, out string playerId, out Int64 userId, out string authToken, out string appVersion, out string masterDataVersion, bool exceptRedisCheck)
    {
        playerId = string.Empty;
        authToken = string.Empty;
        appVersion = string.Empty;
        masterDataVersion = string.Empty;
        userId = 0;

        try
        {
            // Request Path에 따라 PlayerID 혹은 UserID를 가져온다.
            if (!ValidateIDByPath(doc, exceptRedisCheck, out playerId, out userId))
            {
                return false;
            }
            
            // 토큰 정보를 가져온다.
            if (!GetTokenString(doc, out authToken))
            {
                return false;
            }

            // 버전 정보를 가져온다.
            if (!GetVersionString(doc, out appVersion, out masterDataVersion))
            {
                return false;
            }

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_GetRequiredField), "Failed");

            return false;
        }
    }

    bool ValidateIDByPath(JsonDocument doc, bool exceptPath, out string playerId, out Int64 userId)
    {
        playerId = string.Empty;
        userId = 0;

        if (exceptPath)
        {
            return ValidateID(doc, out playerId);
        }
        else
        {
            return ValidateID(doc, out userId);
        }
    }

    bool ValidateJSONFormat(string rawBody, out JsonDocument? doc)
    {
        doc = null;
        try
        {
            doc = JsonDocument.Parse(rawBody);

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_ValidateJSONFormat), "Failed");

            return false;
        }
    }

    bool ValidateID(JsonDocument doc, out Int64 userId)
    {
        userId = 0;

        try
        {
            userId = doc.RootElement.GetProperty("UserID").GetInt64();

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_ValidateUserID), "Failed");

            return false;
        }
    }

    bool ValidateID(JsonDocument doc, out string playerId)
    {
        playerId = string.Empty;

        try
        {
            var playerIdString = doc.RootElement.GetProperty("PlayerID").GetString();

            if (playerIdString == null)
            {
                return false;
            }

            playerId = playerIdString;

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_ValidatePlayerID), "Failed");

            return false;
        }
    }

    bool GetTokenString(JsonDocument doc, out string authToken)
    {
        authToken = string.Empty;
        try
        {
            var authTokenString = doc.RootElement.GetProperty("AuthToken").GetString();

            if (authTokenString == null || authTokenString == string.Empty)
            {
                return false;
            }

            authToken = authTokenString;

            return true;

        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_GetTokenString), "Failed");

            return false;
        }
    }

    bool GetVersionString(JsonDocument doc, out string appVersion, out string masterDataVersion)
    {
        appVersion = string.Empty;
        masterDataVersion = string.Empty;

        try
        {
            var appVersionString = doc.RootElement.GetProperty("AppVersion").GetString();
            var masterDataVersionString = doc.RootElement.GetProperty("MasterDataVersion").GetString();

            if (appVersionString == null || appVersionString == string.Empty)
            {
                return false;
            }

            if (masterDataVersionString == null || masterDataVersionString == string.Empty)
            {
                return false;
            }

            appVersion = appVersionString;
            masterDataVersion = masterDataVersionString;

            return true;

        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.AuthCheck_Fail_GetVersionString), "Failed");

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