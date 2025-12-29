using InvoiceService.Application.Abstractions;
using MassTransit;
using Shared.Contracts.Events;

namespace InvoiceService.Infrastructure.Messaging
{
    public class EfOutboxEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publish;
        public EfOutboxEventPublisher(IPublishEndpoint publish) => _publish = publish;

        public Task PublishInvoiceSubmittedAsync(InvoiceSubmitted @event, CancellationToken ct)
        {
            return _publish.Publish(@event, ct);
        }
    }
}
