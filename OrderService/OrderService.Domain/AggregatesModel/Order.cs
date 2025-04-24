using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregatesModel
{
	public class Order: IEntity, IAggregateRoot
	{
		public Guid Id { get; private set; }
		public Guid CustomerId { get; private set; }
		public Address ShippingAddress { get; private set; }
		public DateTime OrderDate { get; private set; }
		public List<OrderItem> Items { get; private set; }
		public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);

		private Order() { } // for EF

		public Order(Guid customerId, Address shippingAddress)
		{
			Id = Guid.NewGuid();
			CustomerId = customerId;
			ShippingAddress = shippingAddress;
			OrderDate = DateTime.UtcNow;
			Items = new List<OrderItem>();
		}

		public void AddItem(Guid productId, int quantity, decimal unitPrice)
		{
			if (quantity <= 0)
				throw new ArgumentException("Quantity must be greater than 0");
			if (unitPrice <= 0)
				throw new ArgumentException("UnitPrice must be greater than 0");

			Items.Add(new OrderItem(productId, quantity, unitPrice));
		}
	}
}
