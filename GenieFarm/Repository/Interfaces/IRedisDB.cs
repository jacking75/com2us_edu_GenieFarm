public interface IRedisDb
{
    public Task<Boolean> SetAsync(String key, String value, TimeSpan? expiry = null);
    public Task<Boolean> SetAsync(String key, Int64 value, TimeSpan? expiry = null);
    public Task<string?> GetAsync(String key);
    public Task<Boolean> DeleteAsync(String key);
    public Task<Boolean> AcquireRequest(String authToken, String path);
    public Task<Boolean> ReleaseRequest(String authToken, String path);
}