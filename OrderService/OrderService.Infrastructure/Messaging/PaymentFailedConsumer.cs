using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Payment;

namespace OrderService.Infrastructure.Messaging
{
	public class PaymentFailedConsumer : IConsumer<PaymentFailed>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<PaymentFailedConsumer> _logger;

		public PaymentFailedConsumer(IOrderEventConsumer handler, ILogger<PaymentFailedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PaymentFailed> context)
		{
			_logger.LogInformation("Received PaymentFailed for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandlePaymentFailedAsync(context.Message, context.CancellationToken);
		}
	}
}

