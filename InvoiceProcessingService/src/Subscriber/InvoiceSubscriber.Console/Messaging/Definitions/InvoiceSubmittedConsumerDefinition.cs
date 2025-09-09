using InvoiceSubscriber.Console.Messaging.Consumers;
using InvoiceSubscriber.Console.Options;
using MassTransit;
using Microsoft.Extensions.Options;

namespace InvoiceSubscriber.Console.Messaging.Definitions
{
	public class InvoiceSubmittedConsumerDefinition : ConsumerDefinition<InvoiceSubmittedConsumer>
	{
		private readonly RabbitMqOptions _opt;

		public InvoiceSubmittedConsumerDefinition(IOptions<RabbitMqOptions> opt)
		{
			_opt = opt.Value;
			EndpointName = _opt.InvoiceSubmittedEndpointName;
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
				IConsumerConfigurator<InvoiceSubmittedConsumer> consumerConfigurator)
		{
			endpointConfigurator.PrefetchCount = _opt.PrefetchCount < _opt.ConcurrentMessageLimit
				? _opt.ConcurrentMessageLimit * 2 // Best practice
				: _opt.PrefetchCount;
			endpointConfigurator.UseConcurrencyLimit(_opt.ConcurrentMessageLimit);

			endpointConfigurator.UseMessageRetry(r =>
					r.Exponential(5,
							minInterval: System.TimeSpan.FromSeconds(1),
							maxInterval: System.TimeSpan.FromSeconds(30),
							intervalDelta: System.TimeSpan.FromSeconds(5)));
		}
	}
}
