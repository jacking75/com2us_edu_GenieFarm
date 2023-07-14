public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthCheckMiddleware>();
    }

    public static IApplicationBuilder UseDTOLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DTOLoggingMiddleware>();
    }
}