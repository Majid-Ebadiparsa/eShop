using MediatR;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Payments.Commands
{
	public record InitiatePaymentCommand(Guid OrderId, decimal Amount, string Currency, PaymentMethodType MethodType) : IRequest<Guid>;
}
