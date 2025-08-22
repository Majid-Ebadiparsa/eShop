using DeliveryService.Application.Commands;
using DeliveryService.Application.DTOs;
using MassTransit;
using MediatR;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class OrderReadyToShipConsumer : IConsumer<OrderReadyToShip>
	{
		private readonly IMediator _mediator;
		public OrderReadyToShipConsumer(IMediator mediator) => _mediator = mediator;

		public async Task Consume(ConsumeContext<OrderReadyToShip> context)
		{
			var msg = context.Message;
			var cmd = new CreateShipmentCommand(
					msg.OrderId,
					new AddressDto(msg.Address.Street, msg.Address.City, msg.Address.Zip, msg.Address.Country),
					msg.Items.Select(i => new ShipmentItemDto(i.ProductId, i.Quantity)).ToList()
			);
			var shipmentId = await _mediator.Send(cmd); // Immediately after sending the command, the Handler itself has added ShipmentCreated to the Outbox
		}
	}
}
