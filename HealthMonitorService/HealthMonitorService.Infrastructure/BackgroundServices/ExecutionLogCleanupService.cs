using HealthMonitorService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthMonitorService.Infrastructure.BackgroundServices
{
	/// <summary>
	/// Background service that periodically cleans up old execution logs
	/// </summary>
	public class ExecutionLogCleanupService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<ExecutionLogCleanupService> _logger;
		private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours
		private readonly int _keepLastNLogs = 1000; // Keep last 1000 logs per service

		public ExecutionLogCleanupService(
			IServiceProvider serviceProvider,
			ILogger<ExecutionLogCleanupService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Wait 1 minute after startup before first cleanup
			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await CleanupOldLogs(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error during execution log cleanup");
				}

				await Task.Delay(_cleanupInterval, stoppingToken);
			}
		}

		private async Task CleanupOldLogs(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var repository = scope.ServiceProvider.GetRequiredService<IExecutionLogRepository>();

			_logger.LogInformation("Starting execution log cleanup (keeping last {Count} logs per service)", _keepLastNLogs);

			try
			{
				await repository.CleanupOldLogsAsync(_keepLastNLogs, cancellationToken);
				_logger.LogInformation("Execution log cleanup completed successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to cleanup execution logs");
			}
		}
	}
}

