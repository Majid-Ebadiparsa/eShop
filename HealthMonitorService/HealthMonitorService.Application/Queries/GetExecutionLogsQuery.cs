using HealthMonitorService.Domain.Entities;
using MediatR;

namespace HealthMonitorService.Application.Queries
{
	public record GetExecutionLogsQuery(
		string? ServiceName = null,
		int Limit = 100,
		bool? ExecutionSucceeded = null
	) : IRequest<List<ServiceExecutionLog>>;
}

