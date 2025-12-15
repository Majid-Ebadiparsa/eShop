using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Payments.Commands
{
	public record RefundPaymentCommand(Guid PaymentId, decimal Amount, string Currency) : IRequest<bool>;
}
