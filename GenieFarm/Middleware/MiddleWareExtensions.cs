public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthCheckMiddleware>();
    }

    public static IApplicationBuilder UseJsonFieldCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JsonFieldCheckMiddleware>();
    }

    public static IApplicationBuilder UseVersionCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VersionCheckMiddleware>();
    }
}