using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Inventory;

namespace OrderService.Infrastructure.Messaging
{
	public class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailed>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReservationFailedConsumer> _logger;

		public InventoryReservationFailedConsumer(IOrderEventConsumer handler, ILogger<InventoryReservationFailedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
		{
			_logger.LogInformation("Received InventoryReservationFailed for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleInventoryReservationFailedAsync(context.Message, context.CancellationToken);
		}
	}
}

