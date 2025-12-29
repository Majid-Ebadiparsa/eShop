using DeliveryService.Application.Abstractions;
using DeliveryService.Infrastructure.Projections;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging
{
    public class ShipmentProjectionConsumer : IConsumer<ShipmentCreated>
    {
        private readonly IShipmentProjectionWriter _writer;
        private readonly ILogger<ShipmentProjectionConsumer> _logger;

        public ShipmentProjectionConsumer(IShipmentProjectionWriter writer, ILogger<ShipmentProjectionConsumer> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ShipmentCreated> context)
        {
            _logger.LogInformation("Projecting ShipmentCreated {ShipmentId}", context.Message.ShipmentId);
            return _writer.UpsertShipmentAsync(context.Message.ShipmentId, context.Message.OrderId, context.Message.OccurredAtUtc, context.CancellationToken);
        }
    }
}
