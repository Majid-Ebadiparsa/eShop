using InvoiceSubscriber.Console.Consumers;
using InvoiceSubscriber.Console.Inbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
	services.AddSingleton<IInboxStore>(_ => new SqliteInboxStore("Data Source=/app/inbox/inbox.db"));

	services.AddMassTransit(x =>
	{
		x.AddConsumer<InvoiceSubmittedConsumer>();

		x.UsingRabbitMq((ctx, cfg) =>
		{
			var host = context.Configuration["RabbitMQ:Host"] ?? "localhost";
			var vhost = context.Configuration["RabbitMQ:VirtualHost"] ?? "/";
			var user = context.Configuration["RabbitMQ:Username"] ?? "guest";
			var pass = context.Configuration["RabbitMQ:Password"] ?? "guest";

			cfg.Host(host, vhost, h =>
			{
				h.Username(user);
				h.Password(pass);
			});


			cfg.ReceiveEndpoint("invoice-submitted-console", e =>
			{
				e.PrefetchCount = 16; // Consumption optimization
				e.ConcurrentMessageLimit = 8; // Concurrency control
				e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
				e.ConfigureConsumer<InvoiceSubmittedConsumer>(ctx);
			});
		});
	});
});


await builder.RunConsoleAsync();
