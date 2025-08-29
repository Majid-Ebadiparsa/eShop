using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InvoiceSubscriber.Console.Consumers;
using InvoiceSubscriber.Console.Inbox;


namespace InvoiceSubscriber.Console.Extensions
{
	public static class DependencyInjection
	{
		const string SECTION_NAME = "InboxDb";

		public static IServiceCollection AddInboxStore(
				this IServiceCollection services,
				IConfiguration configuration,
				IHostEnvironment env,
				Action<SqliteInboxOptions>? configure = null)
		{
			services.AddSingleton<IInboxStore>(sp =>
			{
				var options = SqliteInboxOptions.FromConfiguration(configuration, env, SECTION_NAME, configure);
				return new SqliteInboxStore(options);
			});

			return services;
		}

		public static IServiceCollection AddSubscriberMessaging(
				this IServiceCollection services,
				IConfiguration configuration)
		{
			services.AddMassTransit(x =>
			{
				x.AddConsumer<InvoiceSubmittedConsumer>();

				x.UsingRabbitMq((ctx, cfg) =>
				{
					var host = configuration["RabbitMQ:Host"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_HOST") ?? "localhost";
					var vhost = configuration["RabbitMQ:VirtualHost"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_VHOST") ?? "/";
					var user = configuration["RabbitMQ:Username"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_USER") ?? "guest";
					var pass = configuration["RabbitMQ:Password"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_PASS") ?? "guest"; 

					cfg.Host(host, vhost, h =>
					{
						h.Username(user);
						h.Password(pass);
					});

					cfg.ReceiveEndpoint("invoice-submitted-console", e =>
					{
						e.PrefetchCount = 16;              // Consumption optimization
						e.ConcurrentMessageLimit = 8;      // Concurrency control
						e.UseMessageRetry(r => r.Exponential(
								retryLimit: 5,
								minInterval: TimeSpan.FromSeconds(1),
								maxInterval: TimeSpan.FromSeconds(30),
								intervalDelta: TimeSpan.FromSeconds(5)));
						e.ConfigureConsumer<InvoiceSubmittedConsumer>(ctx);
					});
				});
			}).AddMassTransitHostedService(true);

			return services;
		}		
	}
}
