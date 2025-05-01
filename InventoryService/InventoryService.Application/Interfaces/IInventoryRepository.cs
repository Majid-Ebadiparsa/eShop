using InventoryService.Domain.AggregatesModel;
using InventoryService.Domain.SeedWork;

namespace InventoryService.Application.Interfaces
{
	public interface IInventoryRepository : IRepository<InventoryItem>
	{
		Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
	}
}
