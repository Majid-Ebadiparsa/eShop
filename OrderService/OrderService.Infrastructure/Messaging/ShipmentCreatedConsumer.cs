using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;

namespace OrderService.Infrastructure.Messaging
{
	public class ShipmentCreatedConsumer : IConsumer<ShipmentCreated>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<ShipmentCreatedConsumer> _logger;

		public ShipmentCreatedConsumer(IOrderEventConsumer handler, ILogger<ShipmentCreatedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ShipmentCreated> context)
		{
			_logger.LogInformation("Received ShipmentCreated for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleShipmentCreatedAsync(context.Message, context.CancellationToken);
		}
	}
}

