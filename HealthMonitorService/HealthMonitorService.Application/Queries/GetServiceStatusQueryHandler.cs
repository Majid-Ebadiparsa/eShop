using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public class GetServiceStatusQueryHandler
	{
		private readonly IHealthStatusRepository _repository;

		public GetServiceStatusQueryHandler(IHealthStatusRepository repository)
		{
			_repository = repository;
		}

		public async Task<ServiceStatusDto?> Handle(GetServiceStatusQuery query, CancellationToken cancellationToken = default)
		{
			var status = await _repository.GetByServiceNameAsync(query.ServiceName.ToLower(), cancellationToken);
			if (status == null)
				return null;

			var history = await _repository.GetHistoryAsync(status.ServiceName, 100, cancellationToken);

			return new ServiceStatusDto
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
			};
		}
	}
}

