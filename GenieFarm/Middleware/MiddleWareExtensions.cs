public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthCheckMiddleware>();
    }
}