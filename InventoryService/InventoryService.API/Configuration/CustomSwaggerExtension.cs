using Microsoft.OpenApi.Models;

namespace InventoryService.API.Configuration
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
					Title = "InventoryService.API",
					Version = "V1",
					Description = "eShop Demo",
				});

				c.OperationFilter<AddDefaultExampleSchemaFilter>();
			});

			return services;
		}

	}
}
