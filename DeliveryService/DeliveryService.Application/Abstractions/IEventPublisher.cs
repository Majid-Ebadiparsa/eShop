namespace DeliveryService.Application.Abstractions
{
	public interface IEventPublisher
	{
		Task AddAsync<TEvent>(TEvent @event, CancellationToken ct = default)
				where TEvent : class; // Using SharedService.Contracts records
	}
}
