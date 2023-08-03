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
builder.Services.AddTransient<IAuctionService, AuctionService>();

builder.Services.AddSingleton<IRedisDb, RedisDb>();
builder.Services.AddSingleton<IMasterDb, MasterDb>();


builder.Services.AddControllers();

// ZLogger ��� ����
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

// JSON ���� �˻�, �ʿ��� Request Field�� �ִ��� �˻� �� Request Header�� �ٿ���
app.UseJsonFieldCheckMiddleware();
// ���� ��ȿ�� üũ
app.UseVersionCheckMiddleware();
// Token ��ȿ�� üũ, Create�� Login API���� �����Ѵ�.
app.UseAuthCheckMiddleware();
app.UseRouting();
app.MapControllers();

app.Run(app.Configuration["ServerAddress"]);
