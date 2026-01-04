using Consul;

namespace HealthMonitorService.Application.Interfaces
{
	public interface IServiceDiscovery
	{
		Task<List<AgentService>> GetServicesAsync(CancellationToken cancellationToken = default);
	}
}

