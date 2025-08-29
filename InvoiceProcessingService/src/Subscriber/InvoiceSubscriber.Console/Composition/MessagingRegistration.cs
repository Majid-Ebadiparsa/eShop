using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using InvoiceSubscriber.Console.Messaging.Consumers;
using InvoiceSubscriber.Console.Messaging.Definitions;
using InvoiceSubscriber.Console.Options;

namespace InvoiceSubscriber.Console.Composition
{
	public static class MessagingRegistration
	{
		public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration cfg)
		{
			services.Configure<RabbitMqOptions>(cfg.GetSection("RabbitMQ"));

			services.AddMassTransit(x =>
			{
				x.AddConsumer<InvoiceSubmittedConsumer, InvoiceSubmittedConsumerDefinition>();

				x.UsingRabbitMq((ctx, bus) =>
				{
					var opt = ctx.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

					bus.Host(opt.Host, opt.VirtualHost, h =>
					{
						h.Username(opt.Username);
						h.Password(opt.Password);
					});

					bus.ConfigureEndpoints(ctx);
				});
			});

			services.AddMassTransitHostedService(true);
			return services;
		}
	}
}
