namespace DeliveryService.Application.Abstractions
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
