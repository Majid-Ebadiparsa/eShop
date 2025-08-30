using InvoiceSubscriber.Console.Messaging.Consumers;
using InvoiceSubscriber.Console.Messaging.Definitions;
using InvoiceSubscriber.Console.Options;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace InvoiceSubscriber.Console.Composition
{
	public static class MessagingRegistration
	{
		public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration cfg)
		{
			services.Configure<RabbitMqOptions>(cfg.GetSection("RabbitMQ"));

			services.PostConfigure<RabbitMqOptions>(opt =>
			{
				var envHost = Environment.GetEnvironmentVariable("RabbitMQ__Host");
				if (!string.IsNullOrWhiteSpace(envHost))
					opt.Host = envHost;

				var inContainer = string.Equals(
						Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
						"true",
						StringComparison.OrdinalIgnoreCase);

				if (inContainer && IsLocalHost(opt.Host))
				{
					opt.Host = "host.docker.internal";
				}
			});

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

		private static bool IsLocalHost(string host)
		{
			if (string.IsNullOrWhiteSpace(host)) return true;
			host = host.Trim().ToLowerInvariant();
			return host == "localhost" || host == "127.0.0.1" || host == "::1";
		}
	}
}
