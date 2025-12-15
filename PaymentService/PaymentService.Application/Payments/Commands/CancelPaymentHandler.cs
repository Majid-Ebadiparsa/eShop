using MediatR;
using PaymentService.Application.Abstractions;
using SharedService.Contracts.Events.Payment;

namespace PaymentService.Application.Payments.Commands
{
	public class CancelPaymentHandler : IRequestHandler<CancelPaymentCommand, bool>
	{
		private readonly IPaymentReadRepository _reader;
		private readonly IPaymentRepository _repo;
		private readonly IIntegrationEventPublisher _bus;
		public CancelPaymentHandler(IPaymentReadRepository reader, IPaymentRepository repo, IIntegrationEventPublisher bus)
				=> (_reader, _repo, _bus) = (reader, repo, bus);

		public async Task<bool> Handle(CancelPaymentCommand req, CancellationToken ct)
		{
			var payment = await _reader.FindAsync(req.PaymentId, ct)
							?? throw new KeyNotFoundException("Payment not found");

			payment.Cancel(req.Reason);
			await _repo.SaveChangesAsync(ct);

			await _bus.PublishAsync(new PaymentCancelled(payment.OrderId, payment.Id, req.Reason), ct);
			return true;
		}
	}
}
