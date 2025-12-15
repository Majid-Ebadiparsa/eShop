using PaymentService.Application.Abstractions;
using PaymentService.Application.DTOs;
using Polly;

namespace PaymentService.Infrastructure.Psp
{
	public class ResilientPaymentGatewayClient : IPaymentGatewayClient
	{
		private readonly IPaymentGatewayClient _inner;
		private readonly IAsyncPolicy _retry;
		private readonly IAsyncPolicy _breaker;

		public ResilientPaymentGatewayClient(IPaymentGatewayClient inner, IAsyncPolicy retry, IAsyncPolicy breaker)
				=> (_inner, _retry, _breaker) = (inner, retry, breaker);

		public Task<FakePaymentGatewayResult> AuthorizeAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
				=> _breaker.WrapAsync(_retry).ExecuteAsync(() => _inner.AuthorizeAsync(pid, amount, currency, ct));

		public Task<FakePaymentGatewayResult> CaptureAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
				=> _breaker.WrapAsync(_retry).ExecuteAsync(() => _inner.CaptureAsync(pid, amount, currency, ct));

		public Task<FakePaymentGatewayResult> RefundAsync(Guid pid, decimal amount, string currency, CancellationToken ct)
				=> _breaker.WrapAsync(_retry).ExecuteAsync(() => _inner.RefundAsync(pid, amount, currency, ct));
	}
}
