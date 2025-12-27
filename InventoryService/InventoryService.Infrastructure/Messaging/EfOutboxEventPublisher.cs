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

        public Task PublishInventoryReservedAsync(Guid orderId, decimal totalAmount, string currency, CancellationToken cancellationToken)
        {
            var @event = new InventoryReserved(orderId, totalAmount, currency);
            return _publishEndpoint.Publish(@event, cancellationToken);
        }

        public Task PublishInventoryReservationFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken)
        {
            var @event = new InventoryReservationFailed(orderId, reason, DateTime.Now);
            return _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
