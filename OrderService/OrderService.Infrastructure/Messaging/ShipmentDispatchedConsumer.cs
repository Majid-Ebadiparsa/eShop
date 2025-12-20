using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;

namespace OrderService.Infrastructure.Messaging
{
	public class ShipmentDispatchedConsumer : IConsumer<ShipmentDispatched>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<ShipmentDispatchedConsumer> _logger;

		public ShipmentDispatchedConsumer(IOrderEventConsumer handler, ILogger<ShipmentDispatchedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ShipmentDispatched> context)
		{
			_logger.LogInformation("Received ShipmentDispatched for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleShipmentDispatchedAsync(context.Message, context.CancellationToken);
		}
	}
}

