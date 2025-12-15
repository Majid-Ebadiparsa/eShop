namespace PaymentService.Domain.Aggregates
{
	public enum PaymentStatus
	{
		Initiated = 0,
		Authorized = 1,
		Captured = 2,
		Failed = 3,
		Refunded = 4,
		Cancelled = 5
	}
}
