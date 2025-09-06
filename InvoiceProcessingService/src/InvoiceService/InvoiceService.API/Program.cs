using InvoiceService.API.Configuration;
using InvoiceService.Application;
using InvoiceService.Infrastructure.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
.AddAuthorization()
.AddCustomSwagger()
.AddApplication()
.RegisterHealthChecks(builder.Configuration)
.AddInfrastructure(builder.Configuration, builder.Environment)
.RegisterJwtBearer(builder.Configuration)
.AddCustomApiVersioning();


var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
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