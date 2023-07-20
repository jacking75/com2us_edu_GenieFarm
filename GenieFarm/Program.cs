using ZLogger;
using Cysharp.Text;
using IdGen;
using IdGen.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IGameDb, GameDb>();
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

//builder.Logging.AddZLoggerRollingFile((dt, x) => $"../logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024, options =>
//{
//    options.EnableStructuredLogging = true;
//    options.PrefixFormatter = (writer, info) => ZString.Utf8Format(writer, "[{0}]", info.Timestamp.ToLocalTime().DateTime);
//});

var app = builder.Build();

if (!await app.Services.GetService<IMasterDb>()!.Init())
{
    return;
}

app.UseAuthCheckMiddleware();
app.UseRouting();
app.MapControllers();

app.Run(app.Configuration["ServerAddress"]);
