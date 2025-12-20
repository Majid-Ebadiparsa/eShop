using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;

namespace OrderService.Infrastructure.Messaging
{
	public class ShipmentDeliveredConsumer : IConsumer<ShipmentDelivered>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<ShipmentDeliveredConsumer> _logger;

		public ShipmentDeliveredConsumer(IOrderEventConsumer handler, ILogger<ShipmentDeliveredConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ShipmentDelivered> context)
		{
			_logger.LogInformation("Received ShipmentDelivered for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleShipmentDeliveredAsync(context.Message, context.CancellationToken);
		}
	}
}

