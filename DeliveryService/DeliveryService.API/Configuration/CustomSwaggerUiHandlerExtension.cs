namespace DeliveryService.API.Configuration
{
	public static class CustomSwaggerUiHandlerExtension
	{
		public static IApplicationBuilder UseCustomSwaggerUiExceptionHandler(this IApplicationBuilder builder)
		{
			builder.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeliveryService.API V1");
			});

			return builder;
		}
	}
}
