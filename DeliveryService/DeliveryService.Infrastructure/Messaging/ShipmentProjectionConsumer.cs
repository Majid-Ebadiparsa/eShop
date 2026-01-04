using DeliveryService.Application.Abstractions;
using DeliveryService.Infrastructure.Projections;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging
{
    public class ShipmentProjectionConsumer : 
        IConsumer<ShipmentCreated>,
        IConsumer<ShipmentBooked>,
        IConsumer<ShipmentDispatched>,
        IConsumer<ShipmentDelivered>,
        IConsumer<ShipmentFailed>
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
        return _writer.UpsertShipmentAsync(
            context.Message.ShipmentId, 
            context.Message.OrderId,
            context.Message.Street,
            context.Message.City,
            context.Message.Zip,
            context.Message.Country,
            context.Message.OccurredAtUtc, 
            context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<ShipmentBooked> context)
    {
        _logger.LogInformation("Projecting ShipmentBooked {ShipmentId}", context.Message.ShipmentId);
        
        // Update both status and carrier info
        await _writer.UpdateStatusAsync(context.Message.ShipmentId, "BOOKED", context.Message.OccurredAtUtc, context.CancellationToken);
        await _writer.UpdateCarrierInfoAsync(
            context.Message.ShipmentId, 
            context.Message.Carrier, 
            context.Message.TrackingNumber, 
            context.Message.OccurredAtUtc, 
            context.CancellationToken);
    }

        public Task Consume(ConsumeContext<ShipmentDispatched> context)
        {
            _logger.LogInformation("Projecting ShipmentDispatched {ShipmentId}", context.Message.ShipmentId);
            return _writer.UpdateStatusAsync(context.Message.ShipmentId, "DISPATCHED", context.Message.OccurredAtUtc, context.CancellationToken);
        }

        public Task Consume(ConsumeContext<ShipmentDelivered> context)
        {
            _logger.LogInformation("Projecting ShipmentDelivered {ShipmentId}", context.Message.ShipmentId);
            return _writer.UpdateStatusAsync(context.Message.ShipmentId, "DELIVERED", context.Message.OccurredAtUtc, context.CancellationToken);
        }

        public Task Consume(ConsumeContext<ShipmentFailed> context)
        {
            _logger.LogInformation("Projecting ShipmentFailed {ShipmentId}", context.Message.ShipmentId);
            return _writer.UpdateStatusAsync(context.Message.ShipmentId, "FAILED", context.Message.OccurredAtUtc, context.CancellationToken);
        }
    }
}
