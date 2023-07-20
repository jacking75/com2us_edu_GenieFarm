using CloudStructures;
using CloudStructures.Structures;
using StackExchange.Redis;
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

    public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var query = new RedisString<string>(_redisConn, key, expiry);
        return await query.SetAsync(value, expiry);
    }

    public async Task<bool> SetAsync(Int64 key, string value, TimeSpan? expiry)
    {
        var query = new RedisString<string>(_redisConn, key.ToString(), expiry);
        return await query.SetAsync(value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        var query = new RedisString<string>(_redisConn, key, null);
        var result = await query.GetAsync();
        return result.GetValueOrDefault();
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var query = new RedisString<string>(_redisConn, key, null);
        return await query.DeleteAsync();
    }

    public async Task<bool> AcquireRequest(string authToken, string path)
    {
        StringBuilder sb = new StringBuilder(authToken).Append(path);
        var query = new RedisString<string>(_redisConn, sb.ToString(), TimeSpan.FromSeconds(5));
        return await query.SetAsync("", null, StackExchange.Redis.When.NotExists);
    }

    public async Task<bool> ReleaseRequest(string authToken, string path)
    {
        StringBuilder sb = new StringBuilder(authToken).Append(path);
        var query = new RedisString<string>(_redisConn, sb.ToString(), null);
        return await query.DeleteAsync();
    }

    public async Task<bool> CompareMemoryKeyValue(string key, string value)
    {
        // key에 해당하는 value가 메모리에 있는 것과 동일한지 비교
        var query = new RedisString<string>(_redisConn, key, null);
        string? memoryValue = (await query.GetAsync()).GetValueOrDefault();

        if (memoryValue == null || !memoryValue.Equals(value))
        {
            return false;
        }

        return true;
    }
}