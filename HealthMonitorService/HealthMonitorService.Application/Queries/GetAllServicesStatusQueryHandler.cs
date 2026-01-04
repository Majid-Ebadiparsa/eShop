using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public class GetAllServicesStatusQueryHandler
	{
		private readonly IHealthStatusRepository _repository;

		public GetAllServicesStatusQueryHandler(IHealthStatusRepository repository)
		{
			_repository = repository;
		}

		public async Task<List<ServiceStatusDto>> Handle(GetAllServicesStatusQuery query, CancellationToken cancellationToken = default)
		{
			var statuses = await _repository.GetAllAsync(cancellationToken);
			var result = new List<ServiceStatusDto>();

			foreach (var status in statuses)
			{
				var history = await _repository.GetHistoryAsync(status.ServiceName, 100, cancellationToken);

				result.Add(new ServiceStatusDto
				{
					ServiceName = status.ServiceName,
					IsHealthy = status.IsHealthy,
					StatusMessage = status.StatusMessage,
					ResponseTimeMs = status.ResponseTimeMs,
					LastChecked = status.CheckedAt,
					History = history.Select(h => new ServiceHealthHistoryDto
					{
						IsHealthy = h.IsHealthy,
						StatusMessage = h.StatusMessage,
						ResponseTimeMs = h.ResponseTimeMs,
						CheckedAt = h.CheckedAt
					}).ToList()
				});
			}

			return result;
		}
	}
}

