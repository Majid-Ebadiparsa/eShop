namespace SharedService.Contracts.Events.Inventory
{
	public record InventoryReleaseRequested(Guid OrderId, List<OrderItem> Items, DateTime OccurredAtUtc);
}

