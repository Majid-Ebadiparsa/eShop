using InventoryService.Application.Commands;
using InventoryService.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers
{
	public class InventoryController : BaseApiController
	{
		private readonly ILogger<InventoryController> _logger;

		public InventoryController(ILogger<InventoryController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		[HttpGet("{productId:guid}")]
		public async Task<IActionResult> GetByProductId(Guid productId)
		{
			var result = await Mediator.Send(new GetInventoryByProductIdQuery(productId));
			if (result is null)
				return NotFound();

			return Ok(result);
		}

		[HttpPost("seed")]
		public async Task<IActionResult> SeedInventory()
		{
			_logger.LogInformation("Seed inventory endpoint called");
			var count = await Mediator.Send(new SeedInventoryCommand());
			return Ok(new { Message = $"Successfully seeded {count} products", SeededCount = count });
		}
	}
}
