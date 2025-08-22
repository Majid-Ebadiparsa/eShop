using DeliveryService.Domain.SeedWork;

namespace DeliveryService.Domain.AggregatesModel
{
	public sealed class ShipmentItem
	{
		public Guid ProductId { get; }
		public int Quantity { get; }
		public ShipmentItem(Guid productId, int quantity)
		{
			if (quantity <= 0) throw new ArgumentException("Quantity > 0");
			ProductId = productId; Quantity = quantity;
		}
	}
}
