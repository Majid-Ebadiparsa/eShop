using MassTransit;
using MediatR;
using PaymentService.Application.Payments.Commands;
using PaymentService.Domain.Aggregates;
using SharedService.Contracts.Events.Inventory;

namespace PaymentService.Infrastructure.Messaging.Consumer
{
	public class InventoryReservedConsumer : IConsumer<InventoryReserved>
	{
		private readonly IMediator _mediator;
		public InventoryReservedConsumer(IMediator mediator) => _mediator = mediator;

		public Task Consume(ConsumeContext<InventoryReserved> context)
		{
			var msg = context.Message;
			return _mediator.Send(
					new InitiatePaymentCommand(msg.OrderId, msg.TotalAmount, msg.Currency, PaymentMethodType.CARD)
			);
		}
	}
}
