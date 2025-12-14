using PaymentService.Application.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Infrastructure.Psp
{
	public class FakePaymentGatewayClient : IPaymentGatewayClient
	{
		public async Task<FakePaymentGatewayResult> AuthorizeAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
		{			
			var result = new FakePaymentGatewayResult
			(
				Success: true,
				Code: $"AUTH-{pid.ToString()[..8]}",
				ErrorMessage: null!
			);

			return await Task.FromResult(result);
		}

		public async Task<FakePaymentGatewayResult> CaptureAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
		{
			var result = new FakePaymentGatewayResult
			(
				Success: true,
				Code: $"CAP-{pid.ToString()[..8]}",
				ErrorMessage: null!
			);

			return await Task.FromResult(result);			
		}

		public async Task<FakePaymentGatewayResult> RefundAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
		{
			var result = new FakePaymentGatewayResult
			(
				Success: true,
				Code: $"REF-{pid.ToString()[..8]}",
				ErrorMessage: null!
			);

			return await Task.FromResult(result);
		}
			
	}
}
