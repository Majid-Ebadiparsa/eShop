using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InvoiceService.API.Configuration
{
	public static class JwtBearerExtension
	{
		public static IServiceCollection RegisterJwtBearer(this IServiceCollection services, IConfiguration cfg)
		{
			// Auth (JWT)
			var jwtKey = cfg["Jwt:Key"] ?? "super-secret-jwt-key-only-for-demo-and-created-by-majid";
			var jwtIssuer = cfg["Jwt:Issuer"] ?? "InvoiceProcessingService";
			var jwtAudience = cfg["Jwt:Audience"] ?? "InvoiceProcessingService.Clients";

			services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = jwtIssuer,
						ValidAudience = jwtAudience,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
					};
				});

			return services;
		}
	}
}
