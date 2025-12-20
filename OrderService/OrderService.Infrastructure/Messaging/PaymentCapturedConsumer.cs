using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Payment;

namespace OrderService.Infrastructure.Messaging
{
	public class PaymentCapturedConsumer : IConsumer<PaymentCaptured>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<PaymentCapturedConsumer> _logger;

		public PaymentCapturedConsumer(IOrderEventConsumer handler, ILogger<PaymentCapturedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PaymentCaptured> context)
		{
			_logger.LogInformation("Received PaymentCaptured for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandlePaymentCapturedAsync(context.Message, context.CancellationToken);
		}
	}
}

