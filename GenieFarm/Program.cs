using ZLogger;
using Cysharp.Text;
using IdGen;
using IdGen.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IGameDb, GameDb>();
builder.Services.AddTransient<IAuthCheckService, AuthCheckService>();
builder.Services.AddTransient<ILoadDataService, LoadDataService>();
builder.Services.AddTransient<IAttendanceService, AttendanceService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddSingleton<IRedisDb, RedisDb>();
builder.Services.AddSingleton<IMasterDb, MasterDb>();


builder.Services.AddControllers();

// ZLogger 사용 설정
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddZLoggerConsole(options =>
{
    options.EnableStructuredLogging = true;
});

var app = builder.Build();

if (!await app.Services.GetService<IMasterDb>()!.Init())
{
    return;
}

// JSON 포맷 검사, 필요한 Request Field가 있는지 검사 후 Request Header에 붙여줌
app.UseJsonFieldCheckMiddleware();
// 버전 유효성 체크
app.UseVersionCheckMiddleware();
// Token 유효성 체크, Create와 Login API에만 동작한다.
app.UseAuthCheckMiddleware();
app.UseRouting();
app.MapControllers();

app.Run(app.Configuration["ServerAddress"]);
