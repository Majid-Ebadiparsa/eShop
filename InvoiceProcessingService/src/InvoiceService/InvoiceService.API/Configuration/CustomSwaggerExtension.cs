using Microsoft.OpenApi.Models;

namespace InvoiceService.API.Configuration
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
					Title = "InvoiceService.API",
					Version = "V1",
					Description = "Invoice Processing System Demo (IPSD)",
				});
			});

			return services;
		}

	}
}
