using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Infrastructure.BackgroundServices;
using HealthMonitorService.Infrastructure.Persistence;
using HealthMonitorService.Infrastructure.Repositories;
using HealthMonitorService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthMonitorService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
		{
			// Database
			services.AddDbContext<HealthMonitorDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("HealthMonitorDb")));

			// Repositories
			services.AddScoped<IHealthStatusRepository, HealthStatusRepository>();
			services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

			// Services
			services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
			services.AddScoped<IHealthChecker, HttpHealthChecker>();

			// HTTP Client for health checks
			services.AddHttpClient();

			// Background Service
			services.AddHostedService<HealthCheckBackgroundService>();
			services.AddHostedService<ExecutionLogCleanupService>();

			return services;
		}
	}
}

