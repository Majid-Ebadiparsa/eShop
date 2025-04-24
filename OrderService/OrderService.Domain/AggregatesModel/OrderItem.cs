using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregatesModel
{
	public class OrderItem: IEntity
	{
		public Guid ProductId { get; private set; }
		public int Quantity { get; private set; }
		public decimal UnitPrice { get; private set; }

		private OrderItem() { } // for EF

		public OrderItem(Guid productId, int quantity, decimal unitPrice)
		{
			ProductId = productId;
			Quantity = quantity;
			UnitPrice = unitPrice;
		}
	}
}
