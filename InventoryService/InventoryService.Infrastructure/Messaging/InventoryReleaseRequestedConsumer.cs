using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Infrastructure.Messaging
{
	public class InventoryReleaseRequestedConsumer : IConsumer<InventoryReleaseRequested>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReleaseRequestedConsumer> _logger;

		public InventoryReleaseRequestedConsumer(IOrderEventConsumer handler, ILogger<InventoryReleaseRequestedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<InventoryReleaseRequested> context)
		{
			_logger.LogInformation("Received InventoryReleaseRequested for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandleInventoryReleaseRequestedAsync(context.Message, context.CancellationToken);
		}
	}
}

