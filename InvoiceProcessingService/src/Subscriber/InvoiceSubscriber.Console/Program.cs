using InvoiceSubscriber.Console;
using InvoiceSubscriber.Console.Composition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsoleApp
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			await CreateHostBuilder(args).Build().RunAsync();
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
								Host.CreateDefaultBuilder(args)
										.ConfigureAppConfiguration((ctx, cfg) =>
										{
											cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
										 .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
										 .AddEnvironmentVariables();
										})
										.ConfigureServices((ctx, services) =>
										{
											services
											.AddLogging(o => o.AddConsole())
											.AddInboxStore(ctx.Configuration, ctx.HostingEnvironment)
											.AddMessaging(ctx.Configuration);
										});
	}
}
