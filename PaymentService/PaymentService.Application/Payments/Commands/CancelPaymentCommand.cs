using MediatR;

namespace PaymentService.Application.Payments.Commands
{
	public record CancelPaymentCommand(Guid PaymentId, string Reason) : IRequest<bool>;
}
