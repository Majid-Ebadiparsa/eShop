using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using DeliveryService.API.Configuration;
using DeliveryService.Application;
using DeliveryService.Infrastructure;
using SharedService.Consul;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
	.AddCustomEnvironmentSettings(builder.Configuration)
	.AddCustomSwagger()
	.AddMediatR(builder.Configuration)
	.AddInfrastructure(builder.Configuration)
	.AddConsul(builder.Configuration);

var app = builder.Build();

// Health checks
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Consul registration
app.UseConsul(builder.Configuration, app.Lifetime);

// Configure the HTTP request pipeline.
app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseCustomSwaggerUiExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();

