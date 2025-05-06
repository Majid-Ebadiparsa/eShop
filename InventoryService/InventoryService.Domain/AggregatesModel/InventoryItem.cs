using InventoryService.Domain.SeedWork;

namespace InventoryService.Domain.AggregatesModel
{
	public class InventoryItem : BaseEntity, IAggregateRoot
	{
		public Guid ProductId { get; private set; }
		public int Quantity { get; private set; }

		private InventoryItem() { } // For EF

		public InventoryItem(Guid id, Guid productId, int initialQuantity)
		{
			Id = id;
			ProductId = productId;
			Quantity = initialQuantity;
		}

		public void Decrease(int quantity)
		{
			if (quantity <= 0)
				throw new ArgumentException("Quantity must be greater than 0");

			if (Quantity < quantity)
				throw new InvalidOperationException("Not enough inventory");

			Quantity -= quantity;
		}

		public void Increase(int quantity)
		{
			if (quantity <= 0)
				throw new ArgumentException("Quantity must be greater than 0");

			Quantity += quantity;
		}
	}
}
