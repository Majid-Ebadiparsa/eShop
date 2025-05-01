using InventoryService.Domain.AggregatesModel;

namespace InventoryService.Domain.SeedWork
{
	public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
	{
		Task AddAsync(InventoryItem item);
		Task SaveChangesAsync(CancellationToken cancellationToken);		
	}
}
