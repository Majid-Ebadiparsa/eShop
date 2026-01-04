namespace InventoryService.Shared.DTOs
{
	public record InventoryDto(
		Guid ProductId,
		int Available,
		int Reserved,
		DateTime LastUpdated
	);
}

