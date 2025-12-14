namespace PaymentService.Infrastructure.Projections
{
	public class PaymentView
	{
		public Guid PaymentId { get; set; }
		public Guid OrderId { get; set; }
		public string Status { get; set; } = "INITIATED";
		public string? CaptureId { get; set; }
		public string? Reason { get; set; }
	}
}
