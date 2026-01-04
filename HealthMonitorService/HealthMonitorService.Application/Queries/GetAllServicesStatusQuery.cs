using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public record GetAllServicesStatusQuery() : IQuery<List<ServiceStatusDto>>;

	public interface IQuery<out TResult>
	{
	}
}

