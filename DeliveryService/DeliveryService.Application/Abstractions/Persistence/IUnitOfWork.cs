namespace DeliveryService.Application.Abstractions.Persistence
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
