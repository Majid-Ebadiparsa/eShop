using Microsoft.AspNetCore.Mvc;

namespace PaymentService.API.Configuration
{
	public static class ApiVersioningExtension
	{
		public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
		{
			services
				.AddApiVersioning(options =>
				{
					options.DefaultApiVersion = new ApiVersion(1, 0);
					options.AssumeDefaultVersionWhenUnspecified = true;
					options.ReportApiVersions = true;
				})
				.AddVersionedApiExplorer(options =>
				{
					options.GroupNameFormat = "'v'VVV";
					options.SubstituteApiVersionInUrl = true;
				});


			return services;
		}
	}
}
