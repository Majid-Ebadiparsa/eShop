using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;
using System.Linq;

namespace InventoryService.Infrastructure.Messaging
{
    public class InventoryProjectionConsumer : 
        IConsumer<OrderCreatedEvent>,
        IConsumer<InventoryReleaseRequested>,
        IConsumer<InventoryReservationFailed>
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

        public async Task Consume(ConsumeContext<InventoryReleaseRequested> context)
        {
            _logger.LogInformation("InventoryProjectionConsumer releasing inventory for OrderId: {OrderId}", context.Message.OrderId);
            var items = context.Message.Items?.Select(i => (i.ProductId, i.Quantity)).ToList() ?? new();
            await _writer.ReleaseReservedInventoryAsync(context.Message.OrderId, items, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
        {
            _logger.LogInformation("InventoryProjectionConsumer handling reservation failed for OrderId: {OrderId}", context.Message.OrderId);
            // When reservation fails, we don't need to update the projection
            // because the reservation was never completed in the first place
            // The projection will reflect the accurate state from successful operations
        }
    }
}
