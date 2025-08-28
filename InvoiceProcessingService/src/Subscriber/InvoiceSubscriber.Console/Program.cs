using MassTransit;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Events;

var builder = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
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
				e.ConfigureConsumer<InvoiceSubmittedConsumer>(ctx);
			});
		});
	});
});


await builder.RunConsoleAsync();

public class InvoiceSubmittedConsumer : IConsumer<InvoiceSubmittedEvent>
{
	public Task Consume(ConsumeContext<InvoiceSubmittedEvent> context)
	{
		var msg = context.Message;
		Console.WriteLine($"[InvoiceSubscriber] Received InvoiceSubmitted: {msg.InvoiceId} - {msg.Description} - {msg.Supplier} - Due {msg.DueDate:yyyy-MM-dd}");
		foreach (var l in msg.Lines)
		{
			Console.WriteLine($" Line: {l.Description} | Price={l.Price} | Qty={l.Quantity}");
		}
		return Task.CompletedTask;
	}
}
