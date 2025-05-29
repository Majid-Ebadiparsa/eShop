using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderService.Application.Commands;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers
{
	public class OrderController : BaseApiController
	{
		private readonly ILogger<OrderController> _logger;
		public OrderController(ILogger<OrderController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrderAsync([FromBody] PlaceOrderCommand command, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"Request => {JsonConvert.SerializeObject(command)}");

			if (command == null || command.Items == null || !command.Items.Any())
			{
				return BadRequest("Invalid order data.");
			}

			var orderId = await Mediator.Send(command, cancellationToken);

			_logger.LogInformation($"Response => {orderId}");

			//return CreatedAtAction(nameof(GetOrderByIdAsync), new { id = orderId }, orderId);
			return Created($"/api/Order/{orderId}", orderId);
		}


		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetOrderByIdAsync(Guid id)
		{
			var orderDto = await Mediator.Send(new GetOrderByIdQuery(id));

			return orderDto is null
				? NotFound()
				: Ok(orderDto);
		}
	}
}