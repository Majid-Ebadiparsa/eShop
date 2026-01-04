using HealthMonitorService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthMonitorService.API.Controllers
{
	[ApiController]
	[Route("api/health/execution-logs")]
	public class ExecutionLogController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ILogger<ExecutionLogController> _logger;

		public ExecutionLogController(IMediator mediator, ILogger<ExecutionLogController> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		/// <summary>
		/// Get execution logs for all services or a specific service
		/// </summary>
		/// <param name="serviceName">Optional service name to filter</param>
		/// <param name="limit">Number of logs to return (default 100, max 500)</param>
		/// <param name="executionSucceeded">Filter by execution success status</param>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetExecutionLogs(
			[FromQuery] string? serviceName = null,
			[FromQuery] int limit = 100,
			[FromQuery] bool? executionSucceeded = null)
		{
			try
			{
				// Enforce max limit
				if (limit > 500) limit = 500;
				if (limit < 1) limit = 1;

				var query = new GetExecutionLogsQuery(serviceName, limit, executionSucceeded);
				var logs = await _mediator.Send(query);

				return Ok(logs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving execution logs");
				return StatusCode(500, new { error = "Failed to retrieve execution logs" });
			}
		}

		/// <summary>
		/// Get execution logs for a specific service
		/// </summary>
		/// <param name="serviceName">Service name</param>
		/// <param name="limit">Number of logs to return (default 100)</param>
		[HttpGet("{serviceName}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetExecutionLogsForService(
			string serviceName,
			[FromQuery] int limit = 100)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(serviceName))
				{
					return BadRequest(new { error = "Service name is required" });
				}

				// Enforce max limit
				if (limit > 500) limit = 500;
				if (limit < 1) limit = 1;

				var query = new GetExecutionLogsQuery(serviceName, limit, null);
				var logs = await _mediator.Send(query);

				return Ok(logs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving execution logs for service {ServiceName}", serviceName);
				return StatusCode(500, new { error = $"Failed to retrieve execution logs for service {serviceName}" });
			}
		}
	}
}

