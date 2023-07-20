public class JsonFieldCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<JsonFieldCheckMiddleware> _logger;

    public JsonFieldCheckMiddleware(RequestDelegate next, ILogger<JsonFieldCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task Invoke(HttpContext context)
    {
        await _next(context);
    }
}