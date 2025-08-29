using GreenPipes;
using InvoiceSubscriber.Console.Abstractions;
using InvoiceSubscriber.Console.Infrastructure.Inbox;
using InvoiceSubscriber.Console.Messaging.Consumers;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace InvoiceSubscriber.Console
{
	public static class DependencyInjection
	{
		public static IServiceCollection RegisterInboxStore(this IServiceCollection services, IConfiguration cfg)
		{
			var raw = Environment.GetEnvironmentVariable("ConnectionStrings__InboxDb")
								?? cfg.GetConnectionString("InboxDb")
								?? "Data Source=./data/inbox.db;Cache=Shared";

			var csb = new SqliteConnectionStringBuilder(raw);
			if (!Path.IsPathRooted(csb.DataSource))
			{
				var basePath = AppContext.BaseDirectory;
				var dataDir = Path.Combine(basePath, "data");
				Directory.CreateDirectory(dataDir);

				var fileName = Path.GetFileName(csb.DataSource);
				csb.DataSource = Path.Combine(dataDir, fileName);
			}
			var inboxConn = csb.ToString();

			services.AddSingleton<IInboxStore>(_ => new SqliteInboxStore(inboxConn));

			return services;
		}

		public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration cfg)
		{
			services.AddMassTransit(x =>
			{
				x.AddConsumer<InvoiceSubmittedConsumer>();

				x.UsingRabbitMq((ctx, bus) =>
				{
					var host = cfg["RabbitMQ:Host"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_HOST") ?? "localhost";
					var vhost = cfg["RabbitMQ:VirtualHost"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_VHOST") ?? "/";
					var user = cfg["RabbitMQ:Username"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_USER") ?? "guest";
					var pass = cfg["RabbitMQ:Password"] ?? Environment.GetEnvironmentVariable("CLOUDAMQP_PASS") ?? "guest";

					bus.Host(host, vhost, h =>
					{
						h.Username(user);
						h.Password(pass);
					});

					bus.ReceiveEndpoint("invoice-submitted-console", e =>
					{
						e.PrefetchCount = 16;              // Consumption optimization
						e.ConcurrentMessageLimit = 8;      // Concurrency control
						e.UseMessageRetry(r => r.Exponential(5,
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
