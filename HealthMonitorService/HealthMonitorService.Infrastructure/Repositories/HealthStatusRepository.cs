using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.Entities;
using HealthMonitorService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMonitorService.Infrastructure.Repositories
{
	public class HealthStatusRepository : IHealthStatusRepository
	{
		private readonly HealthMonitorDbContext _context;

		public HealthStatusRepository(HealthMonitorDbContext context)
		{
			_context = context;
		}

		public async Task<List<ServiceHealthStatus>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _context.ServiceHealthStatuses
				.AsNoTracking()
				.OrderBy(s => s.ServiceName)
				.ToListAsync(cancellationToken);
		}

		public async Task<ServiceHealthStatus?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default)
		{
			return await _context.ServiceHealthStatuses
				.AsNoTracking()
				.FirstOrDefaultAsync(s => s.ServiceName == serviceName.ToLower(), cancellationToken);
		}

		public async Task<List<ServiceHealthHistory>> GetHistoryAsync(string serviceName, int take = 100, CancellationToken cancellationToken = default)
		{
			return await _context.ServiceHealthHistories
				.AsNoTracking()
				.Where(h => h.ServiceName == serviceName.ToLower())
				.OrderByDescending(h => h.CheckedAt)
				.Take(take)
				.ToListAsync(cancellationToken);
		}

		public async Task SaveStatusAsync(ServiceHealthStatus status, CancellationToken cancellationToken = default)
		{
			var existing = await _context.ServiceHealthStatuses
				.FirstOrDefaultAsync(s => s.ServiceName == status.ServiceName, cancellationToken);

			if (existing != null)
			{
				// Save to history if status changed
				if (existing.IsHealthy != status.IsHealthy)
				{
					_context.ServiceHealthHistories.Add(new ServiceHealthHistory
					{
						ServiceName = existing.ServiceName,
						IsHealthy = existing.IsHealthy,
						StatusMessage = existing.StatusMessage,
						ResponseTimeMs = existing.ResponseTimeMs,
						CheckedAt = existing.CheckedAt,
						ErrorCode = existing.ErrorCode,
						ExceptionType = existing.ExceptionType,
						StackTrace = existing.StackTrace
					});
				}

				existing.IsHealthy = status.IsHealthy;
				existing.StatusMessage = status.StatusMessage;
				existing.ResponseTimeMs = status.ResponseTimeMs;
				existing.CheckedAt = status.CheckedAt;
				existing.ErrorCode = status.ErrorCode;
				existing.ExceptionType = status.ExceptionType;
				existing.StackTrace = status.StackTrace;
			}
			else
			{
				_context.ServiceHealthStatuses.Add(status);
			}

			// Always add to history
			_context.ServiceHealthHistories.Add(new ServiceHealthHistory
			{
				ServiceName = status.ServiceName,
				IsHealthy = status.IsHealthy,
				StatusMessage = status.StatusMessage,
				ResponseTimeMs = status.ResponseTimeMs,
				CheckedAt = status.CheckedAt,
				ErrorCode = status.ErrorCode,
				ExceptionType = status.ExceptionType,
				StackTrace = status.StackTrace
			});

			// Keep only last 1000 records per service
			var historyCount = await _context.ServiceHealthHistories
				.CountAsync(h => h.ServiceName == status.ServiceName, cancellationToken);

			if (historyCount > 1000)
			{
				var oldestRecords = await _context.ServiceHealthHistories
					.Where(h => h.ServiceName == status.ServiceName)
					.OrderBy(h => h.CheckedAt)
					.Take(historyCount - 1000)
					.ToListAsync(cancellationToken);

				_context.ServiceHealthHistories.RemoveRange(oldestRecords);
			}

			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}

