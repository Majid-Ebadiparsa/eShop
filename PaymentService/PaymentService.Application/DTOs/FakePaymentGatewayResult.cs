namespace PaymentService.Application.DTOs
{
	public record FakePaymentGatewayResult
	(
		bool Success,
		string? Code,
		string? ErrorMessage
	);

}
