using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;
using SharedService.Contracts.Events.Payment;

namespace OrderService.Application.Interfaces
{
	public interface IOrderEventConsumer
	{
		Task HandleInventoryReservedAsync(InventoryReserved @event, CancellationToken cancellationToken);
		Task HandleInventoryReservationFailedAsync(InventoryReservationFailed @event, CancellationToken cancellationToken);
		Task HandlePaymentAuthorizedAsync(PaymentAuthorized @event, CancellationToken cancellationToken);
		Task HandlePaymentCapturedAsync(PaymentCaptured @event, CancellationToken cancellationToken);
		Task HandlePaymentFailedAsync(PaymentFailed @event, CancellationToken cancellationToken);
		Task HandleShipmentCreatedAsync(ShipmentCreated @event, CancellationToken cancellationToken);
		Task HandleShipmentDispatchedAsync(ShipmentDispatched @event, CancellationToken cancellationToken);
		Task HandleShipmentDeliveredAsync(ShipmentDelivered @event, CancellationToken cancellationToken);
	}
}

