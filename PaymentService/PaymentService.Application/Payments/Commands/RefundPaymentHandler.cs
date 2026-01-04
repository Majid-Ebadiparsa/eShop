using MediatR;
using PaymentService.Application.Abstractions;
using SharedService.Contracts.Events.Payment;

namespace PaymentService.Application.Payments.Commands
{
	public class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand, bool>
	{
		private readonly IPaymentReadRepository _reader;
		private readonly IPaymentRepository _repo;
		private readonly IPaymentGatewayClient _psp;
		private readonly IIntegrationEventPublisher _bus;

		public RefundPaymentHandler(IPaymentReadRepository reader, IPaymentRepository repo, IPaymentGatewayClient psp, IIntegrationEventPublisher bus)
				=> (_reader, _repo, _psp, _bus) = (reader, repo, psp, bus);

		public async Task<bool> Handle(RefundPaymentCommand req, CancellationToken ct)
		{
			var payment = await _reader.FirstOrDefaultAsync(req.PaymentId, ct)
										 ?? throw new KeyNotFoundException("Payment not found");

			var result = await _psp.RefundAsync(payment.Id, req.Amount, req.Currency, ct);
			if (!result.Success)
				throw new InvalidOperationException(result.ErrorMessage);

			payment.Refund(result.Code!);
			await _repo.SaveChangesAsync(ct);

			await _bus.PublishAsync(new PaymentRefunded(
				payment.OrderId,
				payment.Id,
				result.Code!,
				Guid.NewGuid(),
				payment.OrderId, // Use OrderId as CorrelationId
				null,
				DateTime.UtcNow), ct);
			return true;
		}
	}
}
