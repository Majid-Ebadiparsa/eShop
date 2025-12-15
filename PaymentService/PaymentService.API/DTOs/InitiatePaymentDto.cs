using PaymentService.Domain.Aggregates;

namespace PaymentService.API.DTOs
{
	public record InitiatePaymentDto(Guid OrderId, decimal Amount, string Currency = "EUR", PaymentMethodType MethodType = PaymentMethodType.CARD);
}
