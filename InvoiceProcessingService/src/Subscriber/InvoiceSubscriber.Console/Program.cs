using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvoiceSubscriber.Console.Extensions;

namespace InvoiceSubscriber.ConsoleApp
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var host = Host.CreateDefaultBuilder(args)
				.ConfigureServices((context, services) =>
				{
					services
							.AddInboxStore(context.Configuration, context.HostingEnvironment)
							.AddSubscriberMessaging(context.Configuration);
				})
				.Build();


			await host.RunAsync();
		}
	}
}
