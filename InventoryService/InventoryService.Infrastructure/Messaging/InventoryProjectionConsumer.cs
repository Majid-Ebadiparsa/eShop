using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;
using System.Linq;

namespace InventoryService.Infrastructure.Messaging
{
    public class InventoryProjectionConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IInventoryProjectionWriter _writer;
        private readonly ILogger<InventoryProjectionConsumer> _logger;

        public InventoryProjectionConsumer(IInventoryProjectionWriter writer, ILogger<InventoryProjectionConsumer> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            _logger.LogInformation("InventoryProjectionConsumer projecting OrderCreated {OrderId}", context.Message.OrderId);
            var items = context.Message.Items?.Select(i => (i.ProductId, i.Quantity)).ToList() ?? new();
            await _writer.UpsertInventoryForOrderAsync(context.Message.OrderId, items, context.CancellationToken);
        }
    }
}
