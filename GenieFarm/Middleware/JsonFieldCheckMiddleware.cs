﻿using System.Text.Json;
using ZLogger;

public class JsonFieldCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<JsonFieldCheckMiddleware> _logger;
    // Hive 서버 인증을 위해 PlayerID를 가져와야 하는 Path들
    readonly HashSet<string> _requirePlayerIDPath = new HashSet<string>()
    {
        "/api/auth/login", "/api/auth/create",
    };

    public JsonFieldCheckMiddleware(RequestDelegate next, ILogger<JsonFieldCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // PlayerID를 가져와야 하는 Path인지 확인
        var path = context.Request.Path;
        var requirePlayerId = IsRequirePlayerIDPath(path);

        // 버퍼링 Enable 및 Request Body 가져오기
        var rawBody = await EnableBuffering(context);
        if (rawBody == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        // JSON 형식이 맞는지 확인하고, 파싱 결과를 doc으로 가져옴
        if (!ValidateJSONFormat(rawBody, out var doc))
        {
            context.Response.StatusCode = 400;
            return;
        }

        // path에 따라 분기해서 context의 Header에 PlayerId 혹은 UserId, AuthToken, AppVersion, MasterDataVersion을 추가
        // 다음 Middleware에서 Header에 있는 데이터를 가져와 판단하도록 하기 위함
        // 필수 Field가 하나라도 없다면 400 반환
        if (!SetRequiredFieldOnHeader(doc!, context, requirePlayerId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        await _next(context);
    }

    bool IsRequirePlayerIDPath(PathString path)
    {
        if (_requirePlayerIDPath.Contains(path))
        {
            return true;
        }
        return false;
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
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_ValidateJSONFormat), "Failed");

            return false;
        }
    }

    bool SetRequiredFieldOnHeader(JsonDocument doc, HttpContext context, bool exceptRedisCheck)
    {
        try
        {
            // Request Path에 따라 PlayerID 혹은 UserID를 가져와 헤더에 추가한다.
            if (!SetIDByPathOnHeader(doc, exceptRedisCheck, context))
            {
                return false;
            }

            // 토큰 정보를 가져와 헤더에 추가한다.
            if (!SetTokenStringOnHeader(doc, context))
            {
                return false;
            }

            // 버전 정보를 가져와 헤더에 추가한다.
            if (!SetVersionStringOnHeader(doc, context))
            {
                return false;
            }

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_GetRequiredField), "Failed");

            return false;
        }
    }

    bool SetIDByPathOnHeader(JsonDocument doc, bool exceptPath, HttpContext context)
    {
        if (exceptPath)
        {
            return SetPlayerIDOnHeader(doc, context);
        }
        else
        {
            return SetUserIDOnHeader(doc, context);
        }
    }

    bool SetUserIDOnHeader(JsonDocument doc, HttpContext context)
    {
        try
        {
            var userId = doc.RootElement.GetProperty("UserID").GetInt64();

            if (userId == 0)
            {
                return false;
            }

            context.Request.Headers.Add("UserID", userId.ToString());

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_GetUserID), "Failed");

            return false;
        }
    }

    bool SetPlayerIDOnHeader(JsonDocument doc, HttpContext context)
    {
        try
        {
            var playerId = doc.RootElement.GetProperty("PlayerID").GetString();

            if (playerId == null)
            {
                return false;
            }

            context.Request.Headers.Add("PlayerID", playerId);

            return true;
        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_GetPlayerID), "Failed");

            return false;
        }
    }

    bool SetTokenStringOnHeader(JsonDocument doc, HttpContext context)
    {
        try
        {
            var authToken = doc.RootElement.GetProperty("AuthToken").GetString();

            if (authToken == null || authToken == string.Empty)
            {
                return false;
            }

            context.Request.Headers.Add("AuthToken", authToken);

            return true;

        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_GetTokenString), "Failed");

            return false;
        }
    }

    bool SetVersionStringOnHeader(JsonDocument doc, HttpContext context)
    {
        try
        {
            var appVersion = doc.RootElement.GetProperty("AppVersion").GetString();
            var masterDataVersion = doc.RootElement.GetProperty("MasterDataVersion").GetString();

            if (appVersion == null || appVersion == string.Empty)
            {
                return false;
            }

            if (masterDataVersion == null || masterDataVersion == string.Empty)
            {
                return false;
            }

            context.Request.Headers.Add("AppVersion", appVersion);
            context.Request.Headers.Add("MasterDataVersion", masterDataVersion);

            return true;

        }
        catch
        {
            _logger.ZLogDebug(EventIdGenerator.Create(ErrorCode.JsonFieldCheck_Fail_GetVersionString), "Failed");

            return false;
        }
    }
}