using InvoiceSubscriber.Console.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
		.ConfigureServices((context, services) =>
		{
			services
					.AddInboxStore(context.Configuration, context.HostingEnvironment)
					.AddSubscriberMessaging(context.Configuration); 
		});


await builder.RunConsoleAsync();
