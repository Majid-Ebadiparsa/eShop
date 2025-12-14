namespace SharedService.Contracts.Events.Payment
{
	public record PaymentCancelled(Guid OrderId, Guid PaymentId, string Reason);
}
