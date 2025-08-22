namespace DeliveryService.Domain.SeedWork
{
	public abstract class BaseEntity<TKey>
	{
		public TKey Id { get; protected set; } = default!;
	}

	public abstract class BaseEntity : BaseEntity<Guid> { }
}
