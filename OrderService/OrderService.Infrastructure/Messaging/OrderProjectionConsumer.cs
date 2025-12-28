using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;
using System.Linq;

namespace OrderService.Infrastructure.Messaging
{
    public class OrderProjectionConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IOrderProjectionWriter _writer;
        private readonly ILogger<OrderProjectionConsumer> _logger;

        public OrderProjectionConsumer(IOrderProjectionWriter writer, ILogger<OrderProjectionConsumer> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            _logger.LogInformation("Projecting OrderCreatedEvent for {OrderId}", context.Message.OrderId);

            var items = context.Message.Items?.Select(i => (i.ProductId, i.Quantity, i.UnitPrice)).ToList() ?? new();

            await _writer.UpsertOrderAsync(context.Message.OrderId, items, context.CancellationToken);
        }
    }
}
