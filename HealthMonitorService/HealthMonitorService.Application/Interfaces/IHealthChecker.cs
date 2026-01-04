using HealthMonitorService.Domain.Entities;

namespace HealthMonitorService.Application.Interfaces
{
	public interface IHealthChecker
	{
		Task<ServiceHealthStatus> CheckServiceHealthAsync(string serviceName, string address, int port, CancellationToken cancellationToken = default);
	}
}

