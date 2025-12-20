using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregatesModel
{
	public class Order: BaseEntity, IAggregateRoot
	{
		public Guid CustomerId { get; private set; }
		public Address ShippingAddress { get; private set; }
		public DateTime OrderDate { get; private set; }
		public List<OrderItem> Items { get; private set; }
		public OrderStatus Status { get; private set; }
		public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);

		private Order() { } // for EF

		public Order(Guid customerId, Address shippingAddress)
		{
			Id = Guid.NewGuid();
			CustomerId = customerId;
			ShippingAddress = shippingAddress;
			OrderDate = DateTime.UtcNow;
			Items = new List<OrderItem>();
			Status = OrderStatus.Pending;
		}

		public void AddItem(Guid productId, int quantity, decimal unitPrice)
		{
			if (quantity <= 0)
				throw new ArgumentException("Quantity must be greater than 0");
			if (unitPrice <= 0)
				throw new ArgumentException("UnitPrice must be greater than 0");

			Items.Add(new OrderItem(productId, quantity, unitPrice));
		}

		public void MarkInventoryReserved()
		{
			if (Status != OrderStatus.Pending)
				throw new InvalidOperationException($"Cannot mark inventory reserved. Current status: {Status}");
			Status = OrderStatus.InventoryReserved;
		}

		public void MarkInventoryReservationFailed()
		{
			if (Status != OrderStatus.Pending)
				throw new InvalidOperationException($"Cannot mark inventory reservation failed. Current status: {Status}");
			Status = OrderStatus.InventoryReservationFailed;
		}

		public void MarkPaymentAuthorized()
		{
			if (Status != OrderStatus.InventoryReserved)
				throw new InvalidOperationException($"Cannot mark payment authorized. Current status: {Status}");
			Status = OrderStatus.PaymentAuthorized;
		}

		public void MarkPaymentCaptured()
		{
			if (Status != OrderStatus.PaymentAuthorized && Status != OrderStatus.InventoryReserved)
				throw new InvalidOperationException($"Cannot mark payment captured. Current status: {Status}");
			Status = OrderStatus.PaymentCaptured;
		}

		public void MarkPaymentFailed()
		{
			if (Status != OrderStatus.InventoryReserved && Status != OrderStatus.PaymentAuthorized)
				throw new InvalidOperationException($"Cannot mark payment failed. Current status: {Status}");
			Status = OrderStatus.PaymentFailed;
		}

		public void MarkShipmentCreated()
		{
			if (Status != OrderStatus.PaymentCaptured)
				throw new InvalidOperationException($"Cannot mark shipment created. Current status: {Status}");
			Status = OrderStatus.ShipmentCreated;
		}

		public void MarkShipmentDispatched()
		{
			if (Status != OrderStatus.ShipmentCreated)
				throw new InvalidOperationException($"Cannot mark shipment dispatched. Current status: {Status}");
			Status = OrderStatus.ShipmentDispatched;
		}

		public void MarkDelivered()
		{
			if (Status != OrderStatus.ShipmentDispatched && Status != OrderStatus.ShipmentCreated)
				throw new InvalidOperationException($"Cannot mark delivered. Current status: {Status}");
			Status = OrderStatus.Delivered;
		}

		public void Cancel()
		{
			if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
				throw new InvalidOperationException($"Cannot cancel order. Current status: {Status}");
			Status = OrderStatus.Cancelled;
		}
	}
}
