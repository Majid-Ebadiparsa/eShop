using DeliveryService.Infrastructure.Persistence;
using MassTransit;

namespace DeliveryService.Infrastructure.Messaging.Consumers.Definitions
{
	public class OnShipmentCreatedConsumerDefinition
			: ConsumerDefinition<OnShipmentCreatedConsumer>
	{
		const string ENDPOINT_NAME = "delivery-on-shipment-created";

		public OnShipmentCreatedConsumerDefinition()
		{
			EndpointName = ENDPOINT_NAME;
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
				IConsumerConfigurator<OnShipmentCreatedConsumer> consumerConfigurator, IRegistrationContext registration)
		{
			consumerConfigurator.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5))); // retry policy
			consumerConfigurator.UseCircuitBreaker(cb => {
				cb.TripThreshold = 2;
				cb.ActiveThreshold = 10;
				cb.ResetInterval = TimeSpan.FromMinutes(1);
			});
			endpointConfigurator.PrefetchCount = 16; // set prefetch count
			consumerConfigurator.ConcurrentMessageLimit = 8; // set concurrency limit
			endpointConfigurator.UseEntityFrameworkOutbox<DeliveryDbContext>(registration);
		}
	}
}
