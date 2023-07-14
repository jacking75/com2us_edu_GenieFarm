using IdGen;
using Cysharp.Text;
using ZLogger;
using System.Net;
using System.Text.Json;

public class DTOLoggingMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<DTOLoggingMiddleware> _logger;
    readonly IIdGenerator<Int64> _idGenerator;

    public DTOLoggingMiddleware(RequestDelegate next, ILogger<DTOLoggingMiddleware> logger, IIdGenerator<Int64> idGenerator)
    {
        _next = next;
        _logger = logger;
        _idGenerator = idGenerator;
    }

    public async Task Invoke(HttpContext context)
    {
        // 클라 IP 가져오기
        GetClientIP(context, out var clientIP);

        // 요청에 대한 요청 ID 생성
        if (!CreateRequestID(context, out var reqId))
        {
            _logger.ZLogWarningWithPayload(new LogModel { ErrorCode = ErrorCode.CreateRequestIDFail, Url = context.Request.Path, ClientIP = clientIP }, "CreateRequestID");
        }

        // Body를 여러 번 읽기 위해 Buffering 설정
        var rawBody = await EnableBuffering(context);
        if (rawBody == null)
        {
            _logger.ZLogInformationWithPayload(new LogModel { RequestId = reqId, Url = context.Request.Path, ClientIP = clientIP, IsRequest = true, RawBody = string.Empty }, "EnableBuffering");
            context.Response.StatusCode = 400;
            return;
        }

        // Request 정보 로깅
        _logger.ZLogInformationWithPayload(new LogModel { RequestId = reqId, Url = context.Request.Path, ClientIP = clientIP, IsRequest = true, RawBody = rawBody }, "Request From Client");

        await _next(context);
    }


    void GetClientIP(HttpContext context, out string clientIP)
    {
        if (context.Connection.RemoteIpAddress == null)
        {
            clientIP = string.Empty;
            return;
        }

        clientIP = context.Connection.RemoteIpAddress.ToString();
    }

    
    async Task<string?> EnableBuffering(HttpContext context)
    {
        context.Request.EnableBuffering();
        var bodyStream = new StreamReader(context.Request.Body);
        try
        {
            var rawBody = await bodyStream.ReadToEndAsync();
            context.Items.Add("RawBody", rawBody);
            context.Request.Body.Position = 0;
            return rawBody;
        } catch
        {
            return null;
        }
    }


    bool CreateRequestID(HttpContext context, out Int64 createdId)
    {
        try
        {
            var reqId = _idGenerator.CreateId();
            createdId = reqId;
            context.Items.Add("RequestId", reqId);
        } catch
        {
            createdId = 0;
            return false;
        }
        return true;
    }
}
