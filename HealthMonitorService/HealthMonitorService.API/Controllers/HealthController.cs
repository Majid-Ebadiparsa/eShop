using HealthMonitorService.Application.Queries;
using HealthMonitorService.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using SharedService.Caching.Abstractions;

namespace HealthMonitorService.API.Controllers
{
	[ApiController]
	[Route("api/health/services")]
	public class HealthController : ControllerBase
	{
		private readonly GetAllServicesStatusQueryHandler _getAllHandler;
		private readonly GetServiceHistoryQueryHandler _getHistoryHandler;
		private readonly ILogger<HealthController> _logger;
		private readonly IRedisCacheClient _cache;

		private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(20);

		public HealthController(
			GetAllServicesStatusQueryHandler getAllHandler,
			GetServiceHistoryQueryHandler getHistoryHandler,
			ILogger<HealthController> logger,
			IRedisCacheClient cache)
		{
			_getAllHandler = getAllHandler;
			_getHistoryHandler = getHistoryHandler;
			_logger = logger;
			_cache = cache;
		}

		// GET /api/health/services
		[HttpGet]
		public async Task<ActionResult<List<ServiceStatusDto>>> GetServices()
		{
			// Try cache first
			var cacheKey = "health:services:summary";
			var cached = await _cache.GetAsync<List<ServiceStatusDto>>(cacheKey);
			if (cached is not null)
			{
				return Ok(cached);
			}

			var result = await _getAllHandler.Handle(new GetAllServicesStatusQuery());

			// Store in cache with explicit TTL
			await _cache.SetAsync(cacheKey, result, CacheTtl);

			return Ok(result);
		}

		// GET /api/health/services/{serviceName}/history
		[HttpGet("{serviceName}/history")]
		public async Task<ActionResult<List<ServiceHealthHistoryDto>>> GetServiceHistory(string serviceName)
		{
			if (string.IsNullOrWhiteSpace(serviceName))
				return BadRequest("serviceName is required");

			var result = await _getHistoryHandler.Handle(new GetServiceHistoryQuery(serviceName, 1000));

			if (result.Count == 0)
			{
				// Check if service exists by trying to get all services
				var allServices = await _getAllHandler.Handle(new GetAllServicesStatusQuery());
				if (!allServices.Any(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)))
				{
					return NotFound();
				}
			}

			return Ok(result);
		}
	}
}
