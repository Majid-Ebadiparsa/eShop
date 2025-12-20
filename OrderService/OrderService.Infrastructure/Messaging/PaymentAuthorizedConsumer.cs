using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Payment;

namespace OrderService.Infrastructure.Messaging
{
	public class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<PaymentAuthorizedConsumer> _logger;

		public PaymentAuthorizedConsumer(IOrderEventConsumer handler, ILogger<PaymentAuthorizedConsumer> logger)
		{
			_handler = handler;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PaymentAuthorized> context)
		{
			_logger.LogInformation("Received PaymentAuthorized for OrderId: {OrderId}", context.Message.OrderId);
			await _handler.HandlePaymentAuthorizedAsync(context.Message, context.CancellationToken);
		}
	}
}

