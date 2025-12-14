using PaymentService.Application.DTOs;

namespace PaymentService.Application.Abstractions
{
	public interface IPaymentGatewayClient
	{
		Task<FakePaymentGatewayResult> AuthorizeAsync(Guid paymentId, decimal amount, string currency, CancellationToken ct);
		Task<FakePaymentGatewayResult> CaptureAsync(Guid paymentId, decimal amount, string currency, CancellationToken ct);
		Task<FakePaymentGatewayResult> RefundAsync(Guid paymentId, decimal amount, string currency, CancellationToken ct);
	}
}
