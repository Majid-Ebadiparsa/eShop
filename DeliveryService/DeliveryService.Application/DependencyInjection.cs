using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using SharedService.Caching;

namespace DeliveryService.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMediatR(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
			services.AddSharedRedisCache(configuration);
			return services;
		}
	}
}
