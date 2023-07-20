using System.Text.RegularExpressions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAuthCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseWhen((context) =>
        {
            // AuthCheckController가 아니라면 무조건 토큰 검사
            if (!context.Request.Path.StartsWithSegments("/api/auth"))
            {
                return true;
            }

            // AuthCheckController Path여도, 로그아웃이라면 토큰 검사
            if (context.Request.Path.Equals("/api/auth/logout"))
            {
                return true;
            }
            return false;
        }
        , builder =>
        {
            builder.UseMiddleware<AuthCheckMiddleware>();
        });
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