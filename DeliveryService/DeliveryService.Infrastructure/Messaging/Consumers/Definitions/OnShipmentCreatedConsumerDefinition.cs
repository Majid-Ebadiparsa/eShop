using DeliveryService.Infrastructure.Persistence;
using MassTransit;

namespace DeliveryService.Infrastructure.Messaging.Consumers.Definitions
{
	public class OnShipmentCreatedConsumerDefinition
			: ConsumerDefinition<OnShipmentCreatedConsumer>
	{
		const string ENDPOINT_NAME = "delivery-on-shipment-created";
		private readonly IRegistrationContext _registration;

		public OnShipmentCreatedConsumerDefinition(IRegistrationContext registration)
		{
			_registration = registration;
			EndpointName = ENDPOINT_NAME;
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
				IConsumerConfigurator<OnShipmentCreatedConsumer> consumerConfigurator, IRegistrationContext registration)
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
