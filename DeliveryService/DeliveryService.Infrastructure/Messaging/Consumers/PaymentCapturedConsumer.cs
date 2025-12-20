using DeliveryService.Application.Abstractions.Services;
using DeliveryService.Application.Commands;
using DeliveryService.Application.DTOs;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events.Payment;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class PaymentCapturedConsumer : IConsumer<PaymentCaptured>
	{
		private readonly IMediator _mediator;
		private readonly IOrderServiceClient _orderServiceClient;
		private readonly ILogger<PaymentCapturedConsumer> _logger;

		public PaymentCapturedConsumer(
			IMediator mediator,
			IOrderServiceClient orderServiceClient,
			ILogger<PaymentCapturedConsumer> logger)
		{
			_mediator = mediator;
			_orderServiceClient = orderServiceClient;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PaymentCaptured> context)
		{
			var msg = context.Message;
			_logger.LogInformation("Received PaymentCaptured for OrderId: {OrderId}", msg.OrderId);

			var orderDetails = await _orderServiceClient.GetOrderDetailsAsync(msg.OrderId, context.CancellationToken);
			if (orderDetails == null)
			{
				_logger.LogWarning("Order details not found for OrderId: {OrderId}", msg.OrderId);
				return;
			}

			var cmd = new CreateShipmentCommand(
				msg.OrderId,
				new AddressDto(orderDetails.Street, orderDetails.City, orderDetails.PostalCode, "US"),
				orderDetails.Items.Select(i => new ShipmentItemDto(i.ProductId, i.Quantity)).ToList()
			);

			var shipmentId = await _mediator.Send(cmd, context.CancellationToken);
			_logger.LogInformation("Created shipment {ShipmentId} for OrderId: {OrderId}", shipmentId, msg.OrderId);
		}
	}
}

