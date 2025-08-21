using Microsoft.OpenApi.Models;

namespace DeliveryService.API.Configuration
{
	public static class CustomSwaggerExtension
	{
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			// Register the Swagger generator
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "OrderService.API",
					Version = "V1",
					Description = "eShop Demo",
				});
			});

			return services;
		}
	}
}
