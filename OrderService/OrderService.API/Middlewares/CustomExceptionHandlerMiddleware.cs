using System.Net;
using Newtonsoft.Json;

namespace OrderService.API.Middlewares
{
	public class CustomExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

		public CustomExceptionHandlerMiddleware(RequestDelegate next,
			 IWebHostEnvironment env,
			 ILogger<CustomExceptionHandlerMiddleware> logger)
		{
			_next = next;
			_env = env;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			string message;

			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				if (_env.IsDevelopment())
				{
					_logger.LogError(ex, ex.Message);

					var dic = new Dictionary<string, string>
					{
						["Exception"] = ex.Message,
						["StackTrace"] = ex.StackTrace,
					};
					message = JsonConvert.SerializeObject(dic);
				}
				else
				{
					message = "Internal server error!";
				}

				await WriteToResponseAsync();
			}

			async Task WriteToResponseAsync()
			{
				if (context.Response.HasStarted)
					throw new InvalidOperationException("The response has already started, the http status code middleware will not be executed.");

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(message);
			}

		}

	}
}
