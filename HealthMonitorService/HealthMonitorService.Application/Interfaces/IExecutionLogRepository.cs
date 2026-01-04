using HealthMonitorService.Domain.Entities;

namespace HealthMonitorService.Application.Interfaces
{
	/// <summary>
	/// Repository for managing service execution logs
	/// </summary>
	public interface IExecutionLogRepository
	{
		/// <summary>
		/// Saves an execution log entry
		/// </summary>
		Task SaveLogAsync(ServiceExecutionLog log, CancellationToken cancellationToken = default);
		
		/// <summary>
		/// Gets execution logs for a specific service
		/// </summary>
		Task<List<ServiceExecutionLog>> GetLogsForServiceAsync(string serviceName, int limit = 100, CancellationToken cancellationToken = default);
		
		/// <summary>
		/// Gets all execution logs with optional filtering
		/// </summary>
		Task<List<ServiceExecutionLog>> GetAllLogsAsync(int limit = 100, bool? executionSucceeded = null, CancellationToken cancellationToken = default);
		
		/// <summary>
		/// Gets execution logs within a time range
		/// </summary>
		Task<List<ServiceExecutionLog>> GetLogsByTimeRangeAsync(DateTime startTime, DateTime endTime, string? serviceName = null, CancellationToken cancellationToken = default);
		
		/// <summary>
		/// Cleans up old execution logs (keeps last N per service)
		/// </summary>
		Task CleanupOldLogsAsync(int keepLastN = 1000, CancellationToken cancellationToken = default);
	}
}

