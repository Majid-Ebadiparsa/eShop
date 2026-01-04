using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedService.Caching.Abstractions;
using System.Diagnostics;

namespace HealthMonitorService.Infrastructure.BackgroundServices
{
	public class HealthCheckBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<HealthCheckBackgroundService> _logger;
		private readonly IRedisCacheClient? _cache;
		private readonly IPublishEndpoint? _publish;
		private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

		public HealthCheckBackgroundService(
			IServiceProvider serviceProvider,
			ILogger<HealthCheckBackgroundService> logger,
			IRedisCacheClient? cache = null,
			IPublishEndpoint? publish = null)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
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
			var serviceDiscovery = scope.ServiceProvider.GetRequiredService<IServiceDiscovery>();
			var healthChecker = scope.ServiceProvider.GetRequiredService<IHealthChecker>();
			var repository = scope.ServiceProvider.GetRequiredService<IHealthStatusRepository>();
			var executionLogRepository = scope.ServiceProvider.GetRequiredService<IExecutionLogRepository>();

			var services = await serviceDiscovery.GetServicesAsync(cancellationToken);

			foreach (var service in services)
			{
				var stopwatch = Stopwatch.StartNew();
				var executionLog = new ServiceExecutionLog
				{
					ServiceName = service.Service,
					ServiceAddress = service.Address,
					ServicePort = service.Port,
					ExecutionStartedAt = DateTime.UtcNow
				};

				try
				{
					var healthStatus = await healthChecker.CheckServiceHealthAsync(service.Service, service.Address, service.Port, cancellationToken);
					stopwatch.Stop();

					// Update execution log with success
					executionLog.ExecutionCompletedAt = DateTime.UtcNow;
					executionLog.DurationMs = stopwatch.ElapsedMilliseconds;
					executionLog.ExecutionSucceeded = true;
					executionLog.ServiceIsHealthy = healthStatus.IsHealthy;
					executionLog.ServiceResponseTimeMs = healthStatus.ResponseTimeMs;
					
					// Extract HTTP status if available from status message
					if (int.TryParse(healthStatus.ErrorCode, out var statusCode))
					{
						executionLog.HttpStatusCode = statusCode;
					}

					if (!healthStatus.IsHealthy)
					{
						executionLog.ErrorMessage = healthStatus.StatusMessage;
						executionLog.ErrorCode = healthStatus.ErrorCode;
						executionLog.ExceptionType = healthStatus.ExceptionType;
					}

					await repository.SaveStatusAsync(healthStatus, cancellationToken);

					// Invalidate cache
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

					// Publish event
					try
					{
						if (_publish is not null)
						{
							await _publish.Publish(new SharedService.Contracts.Events.ServiceHealthChanged(
								ServiceName: healthStatus.ServiceName,
								IsHealthy: healthStatus.IsHealthy,
								ResponseTimeMs: healthStatus.ResponseTimeMs,
								CheckedAt: healthStatus.CheckedAt,
								MessageId: Guid.NewGuid(),
								CorrelationId: Guid.NewGuid(),
								CausationId: null,
								OccurredAtUtc: DateTime.UtcNow),
								cancellationToken);
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Failed to publish ServiceHealthChanged event");
					}
				}
				catch (Exception ex)
				{
					stopwatch.Stop();

					// Update execution log with failure
					executionLog.ExecutionCompletedAt = DateTime.UtcNow;
					executionLog.DurationMs = stopwatch.ElapsedMilliseconds;
					executionLog.ExecutionSucceeded = false;
					executionLog.ErrorMessage = ex.Message;
					executionLog.ExceptionType = ex.GetType().Name;
					executionLog.StackTrace = ex.StackTrace?.Length > 4000 ? ex.StackTrace.Substring(0, 4000) : ex.StackTrace;

					_logger.LogError(ex, "Error checking health for service {ServiceName}", service.Service);
				}
				finally
				{
					// Always save execution log
					try
					{
						await executionLogRepository.SaveLogAsync(executionLog, cancellationToken);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Failed to save execution log for service {ServiceName}", service.Service);
					}
				}
			}
		}
	}
}

