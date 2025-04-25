using OrderService.API.Middlewares;

namespace OrderService.API.Configuration
{
	public static class CustomExceptionHandlerMiddlewareExtensions
	{
		public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
		}
	}
}
