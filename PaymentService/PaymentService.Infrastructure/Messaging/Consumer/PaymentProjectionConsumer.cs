using MassTransit;
using PaymentService.Application.Abstractions;
using SharedService.Contracts.Events.Payment;

namespace PaymentService.Infrastructure.Messaging.Consumer
{
	public class PaymentProjectionConsumer :
			IConsumer<PaymentAuthorized>, IConsumer<PaymentCaptured>, IConsumer<PaymentFailed>
	{
		private readonly IPaymentProjectionWriter _writer;
		public PaymentProjectionConsumer(IPaymentProjectionWriter writer) => _writer = writer;

		public Task Consume(ConsumeContext<PaymentAuthorized> ctx)
				=> _writer.UpsertAuthorizedAsync(ctx.Message.PaymentId, ctx.Message.OrderId, ctx.CancellationToken);

		public Task Consume(ConsumeContext<PaymentCaptured> ctx)
				=> _writer.SetCapturedAsync(ctx.Message.PaymentId, ctx.Message.CaptureId, ctx.CancellationToken);

		public Task Consume(ConsumeContext<PaymentFailed> ctx)
				=> _writer.SetFailedAsync(ctx.Message.OrderId, ctx.Message.Reason, ctx.CancellationToken);
	}
}
