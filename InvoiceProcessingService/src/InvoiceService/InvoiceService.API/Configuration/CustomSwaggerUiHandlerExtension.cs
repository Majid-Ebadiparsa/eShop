namespace InvoiceService.API.Configuration
{
	public static class CustomSwaggerUiHandlerExtension
	{
		public static IApplicationBuilder UseCustomSwaggerUiExceptionHandler(this IApplicationBuilder builder)
		{
			builder
				.UseSwagger()
				.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvoiceService.API V1");
				c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
			});

			return builder;
		}
	}
}
