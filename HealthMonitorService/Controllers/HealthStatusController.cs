using HealthMonitorService.Data;
using HealthMonitorService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthMonitorService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HealthStatusController : ControllerBase
	{
		private readonly HealthMonitorDbContext _context;
		private readonly ILogger<HealthStatusController> _logger;

		public HealthStatusController(HealthMonitorDbContext context, ILogger<HealthStatusController> logger)
		{
			_context = context;
			_logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<List<ServiceStatusDto>>> GetAllServicesStatus()
		{
			var statuses = await _context.ServiceHealthStatuses.ToListAsync();
			var result = new List<ServiceStatusDto>();

			foreach (var status in statuses)
			{
				var history = await _context.ServiceHealthHistories
					.Where(h => h.ServiceName == status.ServiceName)
					.OrderByDescending(h => h.CheckedAt)
					.Take(100)
					.Select(h => new ServiceHealthHistoryDto
					{
						IsHealthy = h.IsHealthy,
						StatusMessage = h.StatusMessage,
						ResponseTimeMs = h.ResponseTimeMs,
						CheckedAt = h.CheckedAt
					})
					.ToListAsync();

				result.Add(new ServiceStatusDto
				{
					ServiceName = status.ServiceName,
					IsHealthy = status.IsHealthy,
					StatusMessage = status.StatusMessage,
					ResponseTimeMs = status.ResponseTimeMs,
					LastChecked = status.CheckedAt,
					History = history
				});
			}

			return Ok(result);
		}

		[HttpGet("{serviceName}")]
		public async Task<ActionResult<ServiceStatusDto>> GetServiceStatus(string serviceName)
		{
			var status = await _context.ServiceHealthStatuses
				.FirstOrDefaultAsync(s => s.ServiceName == serviceName.ToLower());

			if (status == null)
				return NotFound();

			var history = await _context.ServiceHealthHistories
				.Where(h => h.ServiceName == serviceName.ToLower())
				.OrderByDescending(h => h.CheckedAt)
				.Take(100)
				.Select(h => new ServiceHealthHistoryDto
				{
					IsHealthy = h.IsHealthy,
					StatusMessage = h.StatusMessage,
					ResponseTimeMs = h.ResponseTimeMs,
					CheckedAt = h.CheckedAt
				})
				.ToListAsync();

			return Ok(new ServiceStatusDto
			{
				ServiceName = status.ServiceName,
				IsHealthy = status.IsHealthy,
				StatusMessage = status.StatusMessage,
				ResponseTimeMs = status.ResponseTimeMs,
				LastChecked = status.CheckedAt,
				History = history
			});
		}
	}
}

