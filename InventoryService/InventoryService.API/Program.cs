using InventoryService.API.Configuration;
using InventoryService.Application;
using InventoryService.Infrastructure;
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

Console.WriteLine("ReceiveEndpoint from config = " + builder.Configuration["RabbitMq:ReceiveEndpoint"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCustomExceptionHandler();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseCustomSwaggerUiExceptionHandler();
app.UseAuthorization();
app.MapControllers();


app.Run();


