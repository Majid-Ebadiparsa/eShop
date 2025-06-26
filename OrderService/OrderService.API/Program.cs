using OrderService.API.Configuration;
using OrderService.Application;
using OrderService.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


builder.Services
	.AddCustomEnvironmentSettings(builder.Configuration)
	.AddCustomSwagger()
	.AddMediatR()
	.AddInfrastructure(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseCustomSwaggerUiExceptionHandler();
app.UseAuthorization();
app.MapControllers();


app.Use(async (context, next) =>
{
	var endpoint = context.GetEndpoint();
	await next();
});

app.Run();
