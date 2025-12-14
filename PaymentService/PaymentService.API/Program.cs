using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaymentService.API.Configuration;
using PaymentService.Application;
using PaymentService.Infrastructure;
using SharedService.Consul;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services
	.AddCustomSwagger()
	.AddPaymentApplication()
	.AddPaymentInfrastructure(cfg)
	.RegisterJwtBearer(builder.Configuration)
	.AddCustomApiVersioning()
	.AddConsul(builder.Configuration);

builder.Services.AddControllers();

// Configure the HTTP request pipeline.
var app = builder.Build();

app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.UseConsul(builder.Configuration, app.Lifetime);

app.UseCustomExceptionHandler()
	 .UseCustomSwaggerUiExceptionHandler()
	 .UseHttpsRedirection()
	 .UseAuthentication()
	 .UseAuthorization();

app.MapControllers();

app.Run();
