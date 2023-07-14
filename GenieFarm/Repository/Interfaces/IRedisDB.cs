public interface IRedisDb
{
    public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
    public Task<bool> SetAsync(string key, Int64 value, TimeSpan? expiry = null);
    public Task<string?> GetAsync(string key);
    public Task<bool> DeleteAsync(string key);
    public Task<bool> AcquireRequest(string authToken, string path);
    public Task<bool> ReleaseRequest(string authToken, string path);
}