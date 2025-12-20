using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Inventory;

namespace OrderService.Infrastructure.Messaging
{
	public class InventoryReservedConsumer : IConsumer<InventoryReserved>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReservedConsumer> _logger;

		public InventoryReservedConsumer(IOrderEventConsumer handler, ILogger<InventoryReservedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<InventoryReserved> context)
		{
			_logger.LogInformation("Received InventoryReserved for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleInventoryReservedAsync(context.Message, context.CancellationToken);
		}
	}
}

