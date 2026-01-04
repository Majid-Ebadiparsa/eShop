namespace DeliveryService.Infrastructure.Persistence
{
	public class ProcessedMessage
	{
		public long Id { get; set; }
		public Guid MessageId { get; set; }
		public string ConsumerName { get; set; } = string.Empty;
		public Guid? CorrelationId { get; set; }
		public DateTime ProcessedAt { get; set; }
	}
}

