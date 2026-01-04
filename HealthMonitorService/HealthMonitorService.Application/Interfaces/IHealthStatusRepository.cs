using HealthMonitorService.Domain.Entities;

namespace HealthMonitorService.Application.Interfaces
{
	public interface IHealthStatusRepository
	{
		Task<List<ServiceHealthStatus>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<ServiceHealthStatus?> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default);
		Task<List<ServiceHealthHistory>> GetHistoryAsync(string serviceName, int take = 100, CancellationToken cancellationToken = default);
		Task SaveStatusAsync(ServiceHealthStatus status, CancellationToken cancellationToken = default);
	}
}

