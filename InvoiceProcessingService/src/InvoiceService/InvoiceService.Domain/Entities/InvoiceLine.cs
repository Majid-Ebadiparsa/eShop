using InvoiceService.Domain.SeedWork;

namespace InvoiceService.Domain.Entities
{
	public class InvoiceLine : BaseEntity
	{
		public string Description { get; private set; } = string.Empty;
		public double Price { get; private set; }
		public int Quantity { get; private set; }
		public Guid InvoiceId { get; private set; }
		public Invoice? Invoice { get; private set; }

		private InvoiceLine() { }

		public InvoiceLine(string description, double price, int quantity)
		{
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Description is required", nameof(description));
			if (price <= 0)
				throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive");
			if (quantity <= 0)
				throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");


			Id = Guid.NewGuid();
			Description = description.Trim();
			Price = price;
			Quantity = quantity;
		}
	}
}
