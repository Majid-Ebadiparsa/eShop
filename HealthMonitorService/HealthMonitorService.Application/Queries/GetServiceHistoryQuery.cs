using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public record GetServiceHistoryQuery(string ServiceName, int Take = 1000) : IQuery<List<ServiceHealthHistoryDto>>;
}

