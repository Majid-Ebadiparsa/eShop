using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;
using SharedService.Contracts.Events.Payment;
using System.Linq;

namespace OrderService.Infrastructure.Messaging
{
    public class OrderProjectionConsumer : 
        IConsumer<OrderCreatedEvent>,
        IConsumer<InventoryReserved>,
        IConsumer<InventoryReservationFailed>,
        IConsumer<PaymentAuthorized>,
        IConsumer<PaymentCaptured>,
        IConsumer<PaymentFailed>,
        IConsumer<ShipmentCreated>,
        IConsumer<ShipmentDispatched>,
        IConsumer<ShipmentDelivered>
    {
        private readonly IOrderProjectionWriter _writer;
        private readonly ILogger<OrderProjectionConsumer> _logger;

        public OrderProjectionConsumer(IOrderProjectionWriter writer, ILogger<OrderProjectionConsumer> logger)
        {
            _writer = writer;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            _logger.LogInformation("Projecting OrderCreatedEvent for {OrderId}", context.Message.OrderId);

            var items = context.Message.Items?.Select(i => (i.ProductId, i.Quantity, i.UnitPrice)).ToList() ?? new();

            await _writer.UpsertOrderAsync(
                context.Message.OrderId,
                context.Message.CustomerId,
                context.Message.Street,
                context.Message.City,
                context.Message.PostalCode,
                items,
                context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<InventoryReserved> context)
        {
            _logger.LogInformation("Projecting InventoryReserved for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "INVENTORY_RESERVED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
        {
            _logger.LogInformation("Projecting InventoryReservationFailed for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "INVENTORY_FAILED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<PaymentAuthorized> context)
        {
            _logger.LogInformation("Projecting PaymentAuthorized for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "PAYMENT_AUTHORIZED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<PaymentCaptured> context)
        {
            _logger.LogInformation("Projecting PaymentCaptured for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "PAYMENT_CAPTURED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            _logger.LogInformation("Projecting PaymentFailed for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "PAYMENT_FAILED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<ShipmentCreated> context)
        {
            _logger.LogInformation("Projecting ShipmentCreated for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "SHIPMENT_CREATED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<ShipmentDispatched> context)
        {
            _logger.LogInformation("Projecting ShipmentDispatched for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "SHIPPED", context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<ShipmentDelivered> context)
        {
            _logger.LogInformation("Projecting ShipmentDelivered for OrderId: {OrderId}", context.Message.OrderId);
            await _writer.UpdateStatusAsync(context.Message.OrderId, "DELIVERED", context.CancellationToken);
        }
    }
}
