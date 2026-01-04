using InventoryService.Shared.DTOs;

namespace InventoryService.Application.Interfaces
{
	public interface IInventoryReadRepository
	{
		Task<InventoryDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
		Task<List<InventoryDto>> GetAllAsync(CancellationToken cancellationToken);
		Task<List<InventoryDto>> GetLowStockAsync(int threshold, CancellationToken cancellationToken);
	}
}

