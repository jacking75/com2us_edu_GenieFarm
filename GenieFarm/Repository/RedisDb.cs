using CloudStructures;
using CloudStructures.Structures;
using StackExchange.Redis;
using System.Text;
using ZLogger;

public class RedisDb : IRedisDb
{
    readonly ILogger<RedisDb> _logger;
    readonly IMasterDb _masterDb;
    RedisConnection _redisConn { get; set; } = null!;

    public RedisDb(ILogger<RedisDb> logger, IMasterDb masterDb, IConfiguration configuration)
    {
        _logger = logger;
        _masterDb = masterDb;
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

    public async Task<bool> AcquireRequest(string userId)
    {
        var key = RedisLockKeyGenerator.Create(userId);
        var expiry = _masterDb._definedValueDictionary!["Redis_LockTime"];

        var query = new RedisString<string>(_redisConn, key, TimeSpan.FromSeconds(expiry));
        return await query.SetAsync("", null, StackExchange.Redis.When.NotExists);
    }

    public async Task<bool> ReleaseRequest(string userId)
    {
        var key = RedisLockKeyGenerator.Create(userId);

        var query = new RedisString<string>(_redisConn, key, null);
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