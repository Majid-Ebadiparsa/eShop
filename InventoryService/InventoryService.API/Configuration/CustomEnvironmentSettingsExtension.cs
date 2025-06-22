namespace InventoryService.API.Configuration
{
	public static class CustomEnvironmentSettingsExtension
	{
		public static IServiceCollection AddCustomEnvironmentSettings(this IServiceCollection services, IConfiguration configuration)
		{
			if (configuration is ConfigurationManager configManager)
			{
				configManager
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false)
					.AddEnvironmentVariables();
			}

			return services;
		}
	}
}
