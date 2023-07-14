using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using ZLogger;

public class LoggingResultFilter : IActionFilter
{
    public ILogger<LoggingResultFilter> _logger;

    public LoggingResultFilter(ILogger<LoggingResultFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult result && result.Value is ErrorCodeDTO errResult)
        {
            if (context.HttpContext.Items.TryGetValue("RequestId", out var reqId))
            {
                _logger.ZLogInformationWithPayload(new { RequestId = reqId, Path = context.HttpContext.Request.Path.Value, ErrorCode = errResult.Result }, "OnActionExecuted");
            } else
            {
                _logger.ZLogDebugWithPayload(new { ErrorCode = errResult.Result }, "Failed To Get RequestId");
            }
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
