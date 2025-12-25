using Consul;
using HealthMonitorService.Data;
using HealthMonitorService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedService.Caching.Abstractions;

namespace HealthMonitorService.Services
{
	public class HealthCheckService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<HealthCheckService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IRedisCacheClient? _cache;
		private readonly IPublishEndpoint? _publish;
		private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

		public HealthCheckService(
			IServiceProvider serviceProvider,
			ILogger<HealthCheckService> logger,
			IConfiguration configuration,
			IRedisCacheClient? cache = null,
			IPublishEndpoint? publish = null)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_configuration = configuration;
			_cache = cache;
			_publish = publish;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await CheckServicesHealth(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error checking services health");
				}

				await Task.Delay(_checkInterval, stoppingToken);
			}
		}

		private async Task CheckServicesHealth(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var consulClient = scope.ServiceProvider.GetRequiredService<IConsulClient>();
			var dbContext = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
			var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

			var services = await GetServicesFromConsul(consulClient, cancellationToken);
			if (!services.Any())
			{
				// Fallback to configured services
				services = GetConfiguredServices();
			}

			foreach (var service in services)
			{
				try
				{
					var healthCheck = await CheckServiceHealth(httpClient, service, cancellationToken);
					await SaveHealthStatus(dbContext, healthCheck, cancellationToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error checking health for service {ServiceName}", service.Service);
					await SaveHealthStatus(dbContext, new ServiceHealthStatus
					{
						ServiceName = service.Service,
						IsHealthy = false,
						StatusMessage = ex.Message,
						ResponseTimeMs = 0,
						CheckedAt = DateTime.UtcNow
					}, cancellationToken);
				}
			}
		}

		private async Task<List<AgentService>> GetServicesFromConsul(IConsulClient consulClient, CancellationToken cancellationToken)
		{
			try
			{
				var services = await consulClient.Agent.Services(cancellationToken);
				return services.Response.Values
					.Where(s => s.Tags?.Contains("microservice") == true)
					.ToList();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to get services from Consul, using configured services");
				return new List<AgentService>();
			}
		}

		private List<AgentService> GetConfiguredServices()
		{
			var services = new List<AgentService>();
			var serviceNames = new[] { "orderservice", "inventoryservice", "paymentservice", "deliveryservice" };

			foreach (var serviceName in serviceNames)
			{
				var baseUrl = _configuration[$"Services:{serviceName}:BaseUrl"] ?? $"http://{serviceName}:8080";
				var uri = new Uri(baseUrl);
				services.Add(new AgentService
				{
					ID = serviceName,
					Service = serviceName,
					Address = uri.Host,
					Port = uri.Port == -1 ? 8080 : uri.Port,
					Tags = new[] { "microservice" }
				});
			}

			return services;
		}

		private async Task<ServiceHealthStatus> CheckServiceHealth(HttpClient httpClient, AgentService service, CancellationToken cancellationToken)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			var healthUrl = $"http://{service.Address}:{service.Port}/health/ready";

			try
			{
				var response = await httpClient.GetAsync(healthUrl, cancellationToken);
				stopwatch.Stop();

				return new ServiceHealthStatus
				{
					ServiceName = service.Service,
					IsHealthy = response.IsSuccessStatusCode,
					StatusMessage = response.IsSuccessStatusCode ? "Healthy" : $"HTTP {response.StatusCode}",
					ResponseTimeMs = stopwatch.ElapsedMilliseconds,
					CheckedAt = DateTime.UtcNow
				};
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				return new ServiceHealthStatus
				{
					ServiceName = service.Service,
					IsHealthy = false,
					StatusMessage = ex.Message,
					ResponseTimeMs = stopwatch.ElapsedMilliseconds,
					CheckedAt = DateTime.UtcNow
				};
			}
		}

		private async Task SaveHealthStatus(HealthMonitorDbContext dbContext, ServiceHealthStatus status, CancellationToken cancellationToken)
		{
			var existing = await dbContext.ServiceHealthStatuses
				.FirstOrDefaultAsync(s => s.ServiceName == status.ServiceName, cancellationToken);

			if (existing != null)
			{
				// Save to history if status changed
				if (existing.IsHealthy != status.IsHealthy)
				{
					dbContext.ServiceHealthHistories.Add(new ServiceHealthHistory
					{
						ServiceName = existing.ServiceName,
						IsHealthy = existing.IsHealthy,
						StatusMessage = existing.StatusMessage,
						ResponseTimeMs = existing.ResponseTimeMs,
						CheckedAt = existing.CheckedAt
					});
				}

				existing.IsHealthy = status.IsHealthy;
				existing.StatusMessage = status.StatusMessage;
				existing.ResponseTimeMs = status.ResponseTimeMs;
				existing.CheckedAt = status.CheckedAt;
			}
			else
			{
				dbContext.ServiceHealthStatuses.Add(status);
			}

			// Always add to history
			dbContext.ServiceHealthHistories.Add(new ServiceHealthHistory
			{
				ServiceName = status.ServiceName,
				IsHealthy = status.IsHealthy,
				StatusMessage = status.StatusMessage,
				ResponseTimeMs = status.ResponseTimeMs,
				CheckedAt = status.CheckedAt
			});

			// Keep only last 1000 records per service
			var historyCount = await dbContext.ServiceHealthHistories
				.CountAsync(h => h.ServiceName == status.ServiceName, cancellationToken);

			if (historyCount > 1000)
			{
				var oldestRecords = await dbContext.ServiceHealthHistories
					.Where(h => h.ServiceName == status.ServiceName)
					.OrderBy(h => h.CheckedAt)
					.Take(historyCount - 1000)
					.ToListAsync(cancellationToken);

				dbContext.ServiceHealthHistories.RemoveRange(oldestRecords);
			}

			await dbContext.SaveChangesAsync(cancellationToken);

			// Invalidate the cached aggregated health summary (if any)
			try
			{
				if (_cache is not null)
				{
					await _cache.RemoveAsync("health:services:summary");
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to invalidate health cache");
			}

			// Publish integration event so other interested services can react and invalidate their caches
			try
			{
				if (_publish is not null)
				{
					await _publish.Publish(new SharedService.Contracts.Events.ServiceHealthChanged(status.ServiceName, status.IsHealthy, status.ResponseTimeMs, status.CheckedAt), cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to publish ServiceHealthChanged event");
			}
		}
	}
}

