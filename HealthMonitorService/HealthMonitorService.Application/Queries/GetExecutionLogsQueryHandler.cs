using HealthMonitorService.Application.Interfaces;
using HealthMonitorService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthMonitorService.Application.Queries
{
	public class GetExecutionLogsQueryHandler : IRequestHandler<GetExecutionLogsQuery, List<ServiceExecutionLog>>
	{
		private readonly IExecutionLogRepository _repository;
		private readonly ILogger<GetExecutionLogsQueryHandler> _logger;

		public GetExecutionLogsQueryHandler(IExecutionLogRepository repository, ILogger<GetExecutionLogsQueryHandler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<List<ServiceExecutionLog>> Handle(GetExecutionLogsQuery request, CancellationToken cancellationToken)
		{
			try
			{
				if (!string.IsNullOrEmpty(request.ServiceName))
				{
					return await _repository.GetLogsForServiceAsync(request.ServiceName, request.Limit, cancellationToken);
				}

				return await _repository.GetAllLogsAsync(request.Limit, request.ExecutionSucceeded, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving execution logs");
				throw;
			}
		}
	}
}

