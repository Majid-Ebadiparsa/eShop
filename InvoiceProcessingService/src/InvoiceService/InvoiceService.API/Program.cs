using InvoiceService.API.Configuration;
using InvoiceService.Application;
using InvoiceService.Infrastructure.Configuration;
using System.Text.Json.Serialization;
using InvoiceService.Infrastructure.Mongo;
using SharedService.Consul;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
//builder.Configuration
//	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//	.AddJsonFile($"appsettings.{builder.Environment}.json", optional: true)
//	.AddEnvironmentVariables();

builder.Services
.AddAuthorization()
.AddCustomSwagger()
.AddApplication()
.RegisterHealthChecks(builder.Configuration)
.AddInfrastructure(builder.Configuration, builder.Environment)
.AddMongoInfrastructure(builder.Configuration)
.RegisterJwtBearer(builder.Configuration)
.AddCustomApiVersioning()
.AddConsul(builder.Configuration);


var app = builder.Build();

// Health checks
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Consul registration
app.UseConsul(builder.Configuration, app.Lifetime);

app
	.UseCustomExceptionHandler()
	.UseHttpsRedirection()
	.UseCustomSwaggerUiExceptionHandler()
	.UseRouting()
	.UseAuthentication()
	.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program { }