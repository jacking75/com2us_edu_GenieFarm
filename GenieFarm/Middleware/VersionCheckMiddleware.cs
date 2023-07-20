public class VersionCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<VersionCheckMiddleware> _logger;

    public VersionCheckMiddleware(RequestDelegate next, ILogger<VersionCheckMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task Invoke(HttpContext context)
    {
        await _next(context);
    }
}
