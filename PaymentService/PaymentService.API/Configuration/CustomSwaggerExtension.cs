using Microsoft.OpenApi.Models;

namespace PaymentService.API.Configuration
{
	public static class CustomSwaggerExtension
	{
		public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
		{
			// Register the Swagger generator
			services
				.AddEndpointsApiExplorer()
				.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo
					{
						Title = "PaymentService.API",
						Version = "V1",
						Description = "Payment Processing System Demo (PPSD)",
					});

					c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
					{
						Name = "Authorization",
						Type = SecuritySchemeType.Http,
						Scheme = "bearer",
						BearerFormat = "JWT",
						In = ParameterLocation.Header,
						Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'"
					});

					c.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "Bearer"
								}
							},
							Array.Empty<string>()
						}
					});
				});

			return services;
		}
	}
}
