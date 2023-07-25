public interface IRedisDb
{
    public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
    public Task<bool> SetAsync(Int64 key, string value, TimeSpan? expiry = null);
    public Task<string?> GetAsync(string key);
    public Task<bool> DeleteAsync(string key);
    public Task<bool> AcquireRequest(string userId);
    public Task<bool> ReleaseRequest(string userId);
    public Task<bool> CompareMemoryKeyValue(string key, string value);
}