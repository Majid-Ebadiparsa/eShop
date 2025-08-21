using DeliveryService.Application.Abstractions.Services;
using DeliveryService.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers
{
	public class ShipmentsController : BaseApiController
	{
		private readonly ILogger<ShipmentsController> _logger;
		public ShipmentsController(ILogger<ShipmentsController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		[HttpGet("{id:guid}"), ActionName(nameof(GetShipmentByIdAsync))]
		public async Task<ActionResult<ShipmentReadModel?>> GetShipmentByIdAsync(Guid id, CancellationToken ct)
		{
			var result = await Mediator.Send(new GetShipmentByIdQuery(id), ct);
			return result is null ? NotFound() : Ok(result);
		}

		[HttpPost("dispatch/{id:guid}")]
		public async Task<IActionResult> Dispatch(Guid id, [FromServices] IShipmentService svc, CancellationToken ct)
		{
			await svc.MarkDispatchedAsync(id, ct);
			return NoContent();
		}

		[HttpPost("delivered/{id:guid}")]
		public async Task<IActionResult> Delivered(Guid id, [FromServices] IShipmentService svc, CancellationToken ct)
		{
			await svc.MarkDeliveredAsync(id, ct);
			return NoContent();
		}
	}
}
