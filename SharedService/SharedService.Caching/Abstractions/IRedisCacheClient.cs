namespace SharedService.Caching.Abstractions
{
	public interface IRedisCacheClient
	{
		Task<T?> GetAsync<T>(string key);
		Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
		Task RemoveAsync(string key);
	}
}
