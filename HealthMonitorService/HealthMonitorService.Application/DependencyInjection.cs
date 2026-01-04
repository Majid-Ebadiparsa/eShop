using HealthMonitorService.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace HealthMonitorService.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			// Register query handlers
			services.AddScoped<GetAllServicesStatusQueryHandler>();
			services.AddScoped<GetServiceStatusQueryHandler>();
			services.AddScoped<GetServiceHistoryQueryHandler>();

			return services;
		}
	}
}

