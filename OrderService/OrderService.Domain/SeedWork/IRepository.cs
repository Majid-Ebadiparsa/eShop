namespace OrderService.Domain.SeedWork
{
	public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
	{
		IUnitOfWork UnitOfWork { get; }
		IQueryable<TEntity> Table { get; }
		IQueryable<TEntity> TableNoTracking { get; }

		Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
		Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids);
		Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
	}
}
