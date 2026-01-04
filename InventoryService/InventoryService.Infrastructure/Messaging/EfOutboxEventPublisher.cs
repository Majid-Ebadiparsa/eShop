using MassTransit;
using InventoryService.Application.Interfaces;
using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Infrastructure.Messaging
{
    public class EfOutboxEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public EfOutboxEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

    public Task PublishInventoryReservedAsync(
        Guid orderId,
        decimal totalAmount,
        string currency,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        var @event = new InventoryReserved(
            OrderId: orderId,
            TotalAmount: totalAmount,
            Currency: currency,
            MessageId: Guid.NewGuid(),
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: DateTime.UtcNow
        );
        return _publishEndpoint.Publish(@event, cancellationToken);
    }

    public Task PublishInventoryReservationFailedAsync(
        Guid orderId,
        string reason,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        var @event = new InventoryReservationFailed(
            OrderId: orderId,
            Reason: reason,
            MessageId: Guid.NewGuid(),
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: DateTime.UtcNow
        );
        return _publishEndpoint.Publish(@event, cancellationToken);
    }
    }
}
