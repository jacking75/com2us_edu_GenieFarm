using ZLogger;
using Cysharp.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IGameDb, GameDb>();
builder.Services.AddSingleton<IRedisDb, RedisDb>();
builder.Services.AddControllers();
// builder.Services.AddTransient<IMasterDb, MasterDb>();

var app = builder.Build();

app.UseAuthCheckMiddleware();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
