using HealthMonitorService.Application.Queries;
using HealthMonitorService.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HealthMonitorService.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HealthStatusController : ControllerBase
	{
		private readonly GetAllServicesStatusQueryHandler _getAllHandler;
		private readonly GetServiceStatusQueryHandler _getServiceHandler;
		private readonly ILogger<HealthStatusController> _logger;

		public HealthStatusController(
			GetAllServicesStatusQueryHandler getAllHandler,
			GetServiceStatusQueryHandler getServiceHandler,
			ILogger<HealthStatusController> logger)
		{
			_getAllHandler = getAllHandler;
			_getServiceHandler = getServiceHandler;
			_logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<List<ServiceStatusDto>>> GetAllServicesStatus()
		{
			var result = await _getAllHandler.Handle(new GetAllServicesStatusQuery());
			return Ok(result);
		}

		[HttpGet("{serviceName}")]
		public async Task<ActionResult<ServiceStatusDto>> GetServiceStatus(string serviceName)
		{
			var result = await _getServiceHandler.Handle(new GetServiceStatusQuery(serviceName));
			
			if (result == null)
				return NotFound();

			return Ok(result);
		}
	}
}
