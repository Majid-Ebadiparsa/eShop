namespace Shared.Contracts.Events
{
	public sealed class InvoiceLineItem
	{
		public string Description { get; set; } = default!;
		public double Price { get; set; }
		public int Quantity { get; set; }

		public InvoiceLineItem() { }
	};
}
