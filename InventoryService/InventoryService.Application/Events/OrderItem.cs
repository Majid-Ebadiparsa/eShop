namespace InventoryService.Application.Events
{
	public record OrderItem(Guid ProductId, int Quantity);
}
