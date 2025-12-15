using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InvoiceService.API.Configuration
{
	public static class CustomExceptionHandlerMiddlewareExtensions
	{
		public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
		{
			builder.UseExceptionHandler(appError =>
			{
				appError.Run(async context =>
				{
					var feature = context.Features.Get<IExceptionHandlerPathFeature>();
					var exception = feature?.Error;

					var problem = new ProblemDetails
					{
						Title = "An error occurred while processing your request.",
						Instance = feature?.Path
					};

					switch (exception)
					{
						case ValidationException ve:
							problem.Status = StatusCodes.Status400BadRequest;
							problem.Title = "Validation error";
							problem.Detail = ve.Message;
							break;

						case ArgumentException ae:
							problem.Status = StatusCodes.Status400BadRequest;
							problem.Title = "Invalid argument";
							problem.Detail = ae.Message;
							break;

						case UnauthorizedAccessException ue:
							problem.Status = StatusCodes.Status401Unauthorized;
							problem.Title = "Unauthorized";
							problem.Detail = ue.Message;
							break;

						case KeyNotFoundException ke:
							problem.Status = StatusCodes.Status404NotFound;
							problem.Title = "Not Found";
							problem.Detail = ke.Message;
							break;

						default:
							problem.Status = StatusCodes.Status500InternalServerError;
							problem.Title = "Internal Server Error";
							problem.Detail = exception?.Message ?? "An unexpected error occurred.";
							break;
					}

					context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "application/problem+json";
					await context.Response.WriteAsJsonAsync(problem);
				});
			});

			return builder;
		}
	}
}
