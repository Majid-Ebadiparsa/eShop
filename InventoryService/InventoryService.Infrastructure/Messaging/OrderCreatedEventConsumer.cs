using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Messaging
{
	public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<OrderCreatedEventConsumer> _logger;

		public OrderCreatedEventConsumer(IOrderEventConsumer handler, ILogger<OrderCreatedEventConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
		{
			_logger.LogInformation("Received OrderCreatedEvent for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.Handle(context.Message, context.CancellationToken);
		}
	}
}
