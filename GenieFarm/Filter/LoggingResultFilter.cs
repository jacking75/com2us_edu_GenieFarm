using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

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
            //_logger.LogInformation("{0}", );
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
