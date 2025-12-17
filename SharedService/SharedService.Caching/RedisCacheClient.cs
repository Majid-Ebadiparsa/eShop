using SharedService.Caching.Abstractions;
using System.Text.Json;
using StackExchange.Redis;

namespace SharedService.Caching
{
	public class RedisCacheClient : IRedisCacheClient
	{
		private readonly IDatabase _db;
		private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

		public RedisCacheClient(IConnectionMultiplexer multiplexer)
		{
			_db = multiplexer.GetDatabase();
		}

		public async Task<T?> GetAsync<T>(string key)
		{
			var val = await _db.StringGetAsync(key).ConfigureAwait(false);
			if (!val.HasValue) return default;
			
			return JsonSerializer.Deserialize<T>(val.ToString(), _jsonOptions);
		}

		public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
		{
			var json = JsonSerializer.Serialize(value, _jsonOptions);
			return _db.StringSetAsync(key, json, ttl, true);
		}

		public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);
	}
}
