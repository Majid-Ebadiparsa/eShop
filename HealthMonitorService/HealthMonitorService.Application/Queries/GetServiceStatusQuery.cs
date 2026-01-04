using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public record GetServiceStatusQuery(string ServiceName) : IQuery<ServiceStatusDto?>;
}

