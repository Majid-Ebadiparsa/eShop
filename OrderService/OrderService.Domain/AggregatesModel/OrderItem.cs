using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregatesModel
{
	public class OrderItem: BaseEntity
	{
		public Guid ProductId { get; private set; }
		public int Quantity { get; private set; }
		public decimal UnitPrice { get; private set; }

		private OrderItem() { } // for EF

		public OrderItem(Guid productId, int quantity, decimal unitPrice)
		{
			Id = Guid.NewGuid();
			ProductId = productId;
			Quantity = quantity;
			UnitPrice = unitPrice;
		}
	}
}
