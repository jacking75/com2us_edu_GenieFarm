public class VersionCheckMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<VersionCheckMiddleware> _logger;
    readonly IMasterDb _masterDb;

    public VersionCheckMiddleware(RequestDelegate next, ILogger<VersionCheckMiddleware> logger, IMasterDb masterDb)
    {
        _next = next;
        _logger = logger;
        _masterDb = masterDb;
    }

    public async Task Invoke(HttpContext context)
    {
        var appVersion = context.Request.Headers["AppVersion"].ToString();
        var masterDataVersion = context.Request.Headers["MasterDataVersion"].ToString();

        // 앱 버전, 게임 데이터 버전 확인
        if (!VersionCheck(appVersion, masterDataVersion))
        {
            context.Response.StatusCode = 426;
            return;
        }

        await _next(context);
    }

    bool VersionCheck(string appVersion, string masterDataVersion)
    {
        if (!(appVersion.Equals(_masterDb._version!.AppVersion)))
        {
            return false;
        }

        if (!(masterDataVersion.Equals(_masterDb._version!.MasterDataVersion)))
        {
            return false;
        }

        return true;
    }
}
