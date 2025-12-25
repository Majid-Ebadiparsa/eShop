using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services
  .AddOcelot(builder.Configuration)
  .AddConsul();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Basic health endpoints so Consul / HealthMonitor can probe the gateway
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");
await app.UseOcelot();

app.Run();
