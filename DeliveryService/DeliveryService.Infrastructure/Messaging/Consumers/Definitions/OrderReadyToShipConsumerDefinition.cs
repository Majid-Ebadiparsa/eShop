using DeliveryService.Infrastructure.Persistence;
using MassTransit;

namespace DeliveryService.Infrastructure.Messaging.Consumers.Definitions
{
	public class OrderReadyToShipConsumerDefinition
			: ConsumerDefinition<OrderReadyToShipConsumer>
	{
		const string ENDPOINT_NAME = "delivery-order-ready-to-ship";
		private readonly IRegistrationContext _registration;

		public OrderReadyToShipConsumerDefinition(IRegistrationContext registration)
		{
			_registration = registration;
			EndpointName = ENDPOINT_NAME;
		}
		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
				IConsumerConfigurator<OrderReadyToShipConsumer> consumerConfigurator, IRegistrationContext registration)
		{
			// Extra configuration can be added here
			// consumerConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // For example: retry policy
			// consumerConfigurator.UseInMemoryOutbox(); // For example: in-memory outbox
			// endpointConfigurator.PrefetchCount = 16; // Optional: set prefetch count
			// consumerConfigurator.ConcurrentMessageLimit = 8; // Optional: set concurrency limit
			endpointConfigurator.UseEntityFrameworkOutbox<DeliveryDbContext>(_registration);
		}
	}
}
