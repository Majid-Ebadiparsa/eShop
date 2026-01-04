using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.DTOs;

namespace HealthMonitorService.Application.Queries
{
	public class GetServiceHistoryQueryHandler
	{
		private readonly IHealthStatusRepository _repository;

		public GetServiceHistoryQueryHandler(IHealthStatusRepository repository)
		{
			_repository = repository;
		}

		public async Task<List<ServiceHealthHistoryDto>> Handle(GetServiceHistoryQuery query, CancellationToken cancellationToken = default)
		{
			var history = await _repository.GetHistoryAsync(query.ServiceName.ToLower(), query.Take, cancellationToken);

			return history.Select(h => new ServiceHealthHistoryDto
			{
				IsHealthy = h.IsHealthy,
				StatusMessage = h.StatusMessage,
				ResponseTimeMs = h.ResponseTimeMs,
				CheckedAt = h.CheckedAt
			}).ToList();
		}
	}
}

