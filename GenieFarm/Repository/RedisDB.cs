using CloudStructures;
using CloudStructures.Structures;
using System.Text;
using ZLogger;

public class RedisDb : IRedisDb
{
    readonly ILogger<RedisDb> _logger;
    RedisConnection _redisConn { get; set; } = null!;

    public RedisDb(ILogger<RedisDb> logger, IConfiguration configuration)
    {
        _logger = logger;
        var redisAddress = configuration.GetSection("DBConnection")["RedisDb"];
        var redisConfig = new RedisConfig("GenieFarm", redisAddress!);
        _redisConn = new RedisConnection(redisConfig);
    }

    public async Task<Boolean> SetAsync(String key, String value, TimeSpan? expiry = null)
    {
        var query = new RedisString<String>(_redisConn, key, expiry);
        return await query.SetAsync(value, expiry);
    }

    public async Task<bool> SetAsync(string key, Int64 value, TimeSpan? expiry)
    {
        var query = new RedisString<Int64>(_redisConn, key, expiry);
        return await query.SetAsync(value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        var query = new RedisString<string>(_redisConn, key, null);
        var result = await query.GetAsync();
        return result.GetValueOrDefault();
    }

    public async Task<Boolean> DeleteAsync(String key)
    {
        var query = new RedisString<String>(_redisConn, key, null);
        return await query.DeleteAsync();
    }

    public async Task<Boolean> AcquireRequest(String authToken, String path)
    {
        StringBuilder sb = new StringBuilder(authToken).Append(path);
        var query = new RedisString<String>(_redisConn, sb.ToString(), TimeSpan.FromSeconds(5));
        return await query.SetAsync("", null, StackExchange.Redis.When.NotExists);
    }

    public async Task<Boolean> ReleaseRequest(String authToken, String path)
    {
        StringBuilder sb = new StringBuilder(authToken).Append(path);
        var query = new RedisString<String>(_redisConn, sb.ToString(), null);
        return await query.DeleteAsync();
    }
}