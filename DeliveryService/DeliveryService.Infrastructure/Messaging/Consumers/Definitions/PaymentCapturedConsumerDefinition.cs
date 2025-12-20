using DeliveryService.Infrastructure.Persistence;
using MassTransit;

namespace DeliveryService.Infrastructure.Messaging.Consumers.Definitions
{
	public class PaymentCapturedConsumerDefinition : ConsumerDefinition<PaymentCapturedConsumer>
	{
		const string ENDPOINT_NAME = "delivery-payment-captured";

		public PaymentCapturedConsumerDefinition()
		{
			EndpointName = ENDPOINT_NAME;
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PaymentCapturedConsumer> consumerConfigurator, IRegistrationContext registration)
		{
			consumerConfigurator.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
			consumerConfigurator.UseCircuitBreaker(cb =>
			{
				cb.TripThreshold = 2;
				cb.ActiveThreshold = 10;
				cb.ResetInterval = TimeSpan.FromMinutes(1);
			});
			endpointConfigurator.PrefetchCount = 16;
			consumerConfigurator.ConcurrentMessageLimit = 8;
			endpointConfigurator.UseEntityFrameworkOutbox<DeliveryDbContext>(registration);
		}
	}
}

