using InvoiceService.API.Configuration;
using InvoiceService.Application;
using InvoiceService.Infrastructure;
using InvoiceService.Infrastructure.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
.AddAuthorization()
.AddCustomSwagger()
.AddSwaggerGen()
.AddApplication()
.RegisterHealthChecks(builder.Configuration)
.AddInfrastructure(builder.Configuration, builder.Environment)
.RegisterJwtBearer(builder.Configuration)
.AddEndpointsApiExplorer()
.AddCustomApiVersioning();


var app = builder.Build();

app.UseInfrastructure();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app
	.UseCustomExceptionHandler()
	.UseHttpsRedirection()
	.UseSwagger()
	.UseCustomSwaggerUiExceptionHandler()
	.UseRouting()
	.UseAuthentication()
	.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program { }