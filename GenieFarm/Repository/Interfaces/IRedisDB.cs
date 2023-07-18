public interface IRedisDb
{
    public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
    public Task<bool> SetAsync(Int64 key, string value, TimeSpan? expiry = null);
    public Task<string?> GetAsync(string key);
    public Task<bool> DeleteAsync(string key);
    public Task<bool> AcquireRequest(string authToken, string path);
    public Task<bool> ReleaseRequest(string authToken, string path);
    public Task<bool> DeleteSessionDataAsync(string authId, string authToken, Int64 userId);
}