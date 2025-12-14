namespace SharedService.Contracts.Events.Payment
{
	public record PaymentRefunded(Guid OrderId, Guid PaymentId, string RefundId);
}
