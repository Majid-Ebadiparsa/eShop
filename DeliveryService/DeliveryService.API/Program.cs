using DeliveryService.API.Configuration;
using DeliveryService.Application;
using DeliveryService.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
	.AddCustomEnvironmentSettings(builder.Configuration)
	.AddCustomSwagger()
	.AddMediatR()
	.AddInfrastructure(builder.Configuration);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseCustomSwaggerUiExceptionHandler();
app.UseAuthorization();
app.MapControllers();


app.Run();

