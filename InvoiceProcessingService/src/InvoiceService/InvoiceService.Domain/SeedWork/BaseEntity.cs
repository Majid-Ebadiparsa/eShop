namespace InvoiceService.Domain.SeedWork
{
	public abstract class BaseEntity<TKey> : IEntity
	{
		public TKey Id { get; set; }
	}

	public abstract class BaseEntity : BaseEntity<Guid>
	{
	}
}
