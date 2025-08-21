using DeliveryService.API.Configuration;
using DeliveryService.Application;
using DeliveryService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddCustomEnvironmentSettings(builder.Configuration)
	.AddCustomSwagger()
	.AddMediatR()
	.AddInfrastructure(builder.Configuration);

// Add services to the container.

var app = builder.Build();


app.Run();

