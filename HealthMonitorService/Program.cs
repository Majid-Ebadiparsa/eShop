using Consul;
using HealthMonitorService.Data;
using HealthMonitorService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<HealthMonitorDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("HealthMonitorDb")));

// Consul
var consulHost = builder.Configuration["Consul:Host"] ?? "localhost";
var consulPort = int.Parse(builder.Configuration["Consul:Port"] ?? "8500");
builder.Services.AddSingleton<IConsulClient>(p => new ConsulClient(config =>
{
	config.Address = new Uri($"http://{consulHost}:{consulPort}");
}));

// Background service
builder.Services.AddHostedService<HealthCheckService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<HealthMonitorDbContext>();
	db.Database.EnsureCreated();
}

app.Run();
