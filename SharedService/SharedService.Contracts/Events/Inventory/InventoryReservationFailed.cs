namespace SharedService.Contracts.Events.Inventory
{
	public record InventoryReservationFailed(Guid OrderId, string Reason, DateTime OccurredAtUtc);
}

