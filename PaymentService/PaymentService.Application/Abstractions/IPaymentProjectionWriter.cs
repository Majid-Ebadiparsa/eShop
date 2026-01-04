using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Abstractions
{
	// PaymentService.Application/Projections/IPaymentProjectionWriter.cs
	public interface IPaymentProjectionWriter
	{
		Task UpsertAuthorizedAsync(Guid paymentId, Guid orderId, CancellationToken ct);
		Task SetCapturedAsync(Guid paymentId, string captureId, CancellationToken ct);
		Task SetFailedAsync(Guid orderId, string reason, CancellationToken ct);
		Task SetRefundedAsync(Guid paymentId, string refundId, CancellationToken ct);
		Task SetCancelledAsync(Guid paymentId, string reason, CancellationToken ct);
	}

}
