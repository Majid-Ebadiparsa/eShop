using System;

namespace OrderService.API.Configuration
{
	public static class CustomEnvironmentSettingsExtension
	{
		public static IServiceCollection AddCustomEnvironmentSettings(this IServiceCollection services, IConfiguration configuration)
		{
			var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

			if (configuration is ConfigurationManager configManager)
			{
				configManager
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false)
					.AddJsonFile($"appsettings.{environment}.json", optional: true)
					.AddEnvironmentVariables();
			}

			return services;
		}
	}
}
