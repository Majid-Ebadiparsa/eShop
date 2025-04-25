using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderService.Application.Commands;

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
		public async Task<IActionResult> CreateOrder([FromBody] PlaceOrderCommand command, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"Request => {JsonConvert.SerializeObject(command)}");

			if (command == null || command.Items == null || !command.Items.Any())
			{
				return BadRequest("Invalid order data.");
			}

			var orderId = await Mediator.Send(command, cancellationToken);

			_logger.LogInformation($"Response => {orderId}");

			return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
		}

		// ToDO: Implement this method to retrieve order details
		[HttpGet("{id}")]
		public IActionResult GetOrderById(Guid id)
		{
			return Ok($"Order Details for {id} (to be implemented)");
		}
	}
}