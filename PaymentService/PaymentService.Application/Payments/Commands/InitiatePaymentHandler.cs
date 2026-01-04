using MediatR;
using PaymentService.Application.Abstractions;
using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;
using SharedService.Contracts.Events.Payment;

namespace PaymentService.Application.Payments.Commands
{
	public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, Guid>
	{
		private readonly IPaymentRepository _repo;
		private readonly IPaymentGatewayClient _psp;
		private readonly IIntegrationEventPublisher _bus;

		public InitiatePaymentHandler(IPaymentRepository repo, IPaymentGatewayClient psp, IIntegrationEventPublisher bus)
		{
			_repo = repo; _psp = psp; _bus = bus;
		}

		public async Task<Guid> Handle(InitiatePaymentCommand req, CancellationToken ct)
		{
			var payment = Payment.Create(req.OrderId, Money.From(req.Amount, req.Currency),
																	 new PaymentMethod(req.MethodType));

			await _repo.AddAsync(payment, ct);
			await _repo.SaveChangesAsync(ct);

			var auth = await _psp.AuthorizeAsync(payment.Id, req.Amount, req.Currency, ct);
			if (!auth.Success)
			{
				payment.MarkFailed("AUTHORIZE", auth.ErrorMessage!);
				await _repo.SaveChangesAsync(ct);
				await _bus.PublishAsync(new PaymentFailed(
					payment.OrderId,
					auth.ErrorMessage!,
					Guid.NewGuid(),
					payment.OrderId,
					null,
					DateTime.UtcNow), ct);
				return payment.Id;
			}

			payment.MarkAuthorized(auth.Code!);
			await _repo.SaveChangesAsync(ct);
			await _bus.PublishAsync(new PaymentAuthorized(
				payment.OrderId,
				payment.Id,
				auth.Code!,
				Guid.NewGuid(),
				payment.OrderId,
				null,
				DateTime.UtcNow), ct);

			var cap = await _psp.CaptureAsync(payment.Id, req.Amount, req.Currency, ct);
			if (!cap.Success)
			{
				payment.MarkFailed("CAPTURE", cap.ErrorMessage!);
				await _repo.SaveChangesAsync(ct);
				await _bus.PublishAsync(new PaymentFailed(
					payment.OrderId,
					cap.ErrorMessage!,
					Guid.NewGuid(),
					payment.OrderId,
					null,
					DateTime.UtcNow), ct);
				return payment.Id;
			}

			payment.MarkCaptured(cap.Code!);
			await _repo.SaveChangesAsync(ct);
			await _bus.PublishAsync(new PaymentCaptured(
				payment.OrderId,
				payment.Id,
				cap.Code!,
				Guid.NewGuid(),
				payment.OrderId,
				null,
				DateTime.UtcNow), ct);

			return payment.Id;
		}
	}
}
