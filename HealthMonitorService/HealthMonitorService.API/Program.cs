using Consul;
using HealthMonitorService.Application;
using HealthMonitorService.Infrastructure;
using HealthMonitorService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using SharedService.Caching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add layered services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Consul
var consulHost = builder.Configuration["Consul:Host"] ?? "localhost";
var consulPort = int.Parse(builder.Configuration["Consul:Port"] ?? "8500");
builder.Services.AddSingleton<IConsulClient>(p => new ConsulClient(config =>
{
	config.Address = new Uri($"http://{consulHost}:{consulPort}");
}));

// Redis cache (shared)
builder.Services.AddSharedRedisCache(builder.Configuration);

// MassTransit (for publishing health change events)
builder.Services.AddMassTransit(x =>
{
	x.UsingRabbitMq((context, cfg) =>
	{
		var rabbit = builder.Configuration.GetSection("RabbitMq");
		var host = rabbit["Host"] ?? "localhost";
		var vhost = rabbit["VirtualHost"] ?? "/";
		var user = rabbit["Username"] ?? "guest";
		var pass = rabbit["Password"] ?? "guest";

		cfg.Host(host, vhost, h =>
		{
			h.Username(user);
			h.Password(pass);
		});
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Serve the simple monitoring UI from wwwroot
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// Fallback to the SPA-like index page for any non-API requests
app.MapFallbackToFile("index.html");

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
	
	try
	{
		logger.LogInformation("Applying database migrations...");
		db.Database.Migrate();
		logger.LogInformation("Database migrations applied successfully");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Error applying database migrations");
		throw;
	}
}

app.Run();
