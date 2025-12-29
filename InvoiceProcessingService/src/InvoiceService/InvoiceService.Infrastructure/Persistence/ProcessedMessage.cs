using System;

namespace InvoiceService.Infrastructure.Persistence
{
    public class ProcessedMessage
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public string ConsumerName { get; set; } = default!;
        public Guid? CorrelationId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
