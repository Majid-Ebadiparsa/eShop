using GreenPipes;
using InvoiceSubscriber.Console.Messaging.Consumers;
using InvoiceSubscriber.Console.Options;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			endpointConfigurator.PrefetchCount = _opt.PrefetchCount;
			endpointConfigurator.UseConcurrencyLimit(_opt.ConcurrentMessageLimit);

			endpointConfigurator.UseMessageRetry(r =>
					r.Exponential(5,
							minInterval: System.TimeSpan.FromSeconds(1),
							maxInterval: System.TimeSpan.FromSeconds(30),
							intervalDelta: System.TimeSpan.FromSeconds(5)));
		}
	}
}
