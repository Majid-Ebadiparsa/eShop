using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedService.Caching.Abstractions;
using StackExchange.Redis;

namespace SharedService.Caching
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddSharedRedisCache(this IServiceCollection services, IConfiguration cfg)
		{
			var redisConn = cfg["Redis:Connection"] ?? "redis:6379";
			var multiplexer = ConnectionMultiplexer.Connect(redisConn);
			services.AddSingleton<IConnectionMultiplexer>(multiplexer);
			services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
			return services;
		}
	}
}
