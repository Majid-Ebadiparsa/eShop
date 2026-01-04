using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.Entities;
using HealthMonitorService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthMonitorService.Infrastructure.Repositories
{
	public class ExecutionLogRepository : IExecutionLogRepository
	{
		private readonly HealthMonitorDbContext _context;
		private readonly ILogger<ExecutionLogRepository> _logger;

		public ExecutionLogRepository(HealthMonitorDbContext context, ILogger<ExecutionLogRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task SaveLogAsync(ServiceExecutionLog log, CancellationToken cancellationToken = default)
		{
			try
			{
				await _context.ServiceExecutionLogs.AddAsync(log, cancellationToken);
				await _context.SaveChangesAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save execution log for service {ServiceName}", log.ServiceName);
				throw;
			}
		}

		public async Task<List<ServiceExecutionLog>> GetLogsForServiceAsync(string serviceName, int limit = 100, CancellationToken cancellationToken = default)
		{
			return await _context.ServiceExecutionLogs
				.Where(l => l.ServiceName == serviceName)
				.OrderByDescending(l => l.ExecutionStartedAt)
				.Take(limit)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<List<ServiceExecutionLog>> GetAllLogsAsync(int limit = 100, bool? executionSucceeded = null, CancellationToken cancellationToken = default)
		{
			var query = _context.ServiceExecutionLogs.AsQueryable();

			if (executionSucceeded.HasValue)
			{
				query = query.Where(l => l.ExecutionSucceeded == executionSucceeded.Value);
			}

			return await query
				.OrderByDescending(l => l.ExecutionStartedAt)
				.Take(limit)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<List<ServiceExecutionLog>> GetLogsByTimeRangeAsync(DateTime startTime, DateTime endTime, string? serviceName = null, CancellationToken cancellationToken = default)
		{
			var query = _context.ServiceExecutionLogs
				.Where(l => l.ExecutionStartedAt >= startTime && l.ExecutionStartedAt <= endTime);

			if (!string.IsNullOrEmpty(serviceName))
			{
				query = query.Where(l => l.ServiceName == serviceName);
			}

			return await query
				.OrderByDescending(l => l.ExecutionStartedAt)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task CleanupOldLogsAsync(int keepLastN = 1000, CancellationToken cancellationToken = default)
		{
			try
			{
				// Get all service names
				var serviceNames = await _context.ServiceExecutionLogs
					.Select(l => l.ServiceName)
					.Distinct()
					.ToListAsync(cancellationToken);

				foreach (var serviceName in serviceNames)
				{
					// Get IDs to keep (last N per service)
					var idsToKeep = await _context.ServiceExecutionLogs
						.Where(l => l.ServiceName == serviceName)
						.OrderByDescending(l => l.ExecutionStartedAt)
						.Take(keepLastN)
						.Select(l => l.Id)
						.ToListAsync(cancellationToken);

					// Delete old logs
					var logsToDelete = await _context.ServiceExecutionLogs
						.Where(l => l.ServiceName == serviceName && !idsToKeep.Contains(l.Id))
						.ToListAsync(cancellationToken);

					if (logsToDelete.Any())
					{
						_context.ServiceExecutionLogs.RemoveRange(logsToDelete);
						await _context.SaveChangesAsync(cancellationToken);
						_logger.LogInformation("Cleaned up {Count} old execution logs for service {ServiceName}", logsToDelete.Count, serviceName);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to cleanup old execution logs");
				throw;
			}
		}
	}
}

