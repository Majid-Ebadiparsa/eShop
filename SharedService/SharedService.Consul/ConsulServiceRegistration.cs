using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharedService.Consul
{
	public static class ConsulServiceRegistration
	{
		public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
		{
			var consulHost = configuration["Consul:Host"] ?? "localhost";
			var consulPort = int.Parse(configuration["Consul:Port"] ?? "8500");

			services.AddSingleton<IConsulClient>(p => new ConsulClient(config =>
			{
				config.Address = new Uri($"http://{consulHost}:{consulPort}");
			}));

			return services;
		}

		public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IConfiguration configuration, IHostApplicationLifetime lifetime)
		{
			var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
			var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ConsulRegistration");
			var registration = CreateServiceRegistration(configuration);

			lifetime.ApplicationStarted.Register(() =>
			{
				logger.LogInformation("Registering service with Consul: {ServiceId}", registration.ID);
				consulClient.Agent.ServiceRegister(registration).Wait();
			});

			lifetime.ApplicationStopping.Register(() =>
			{
				logger.LogInformation("Deregistering service from Consul: {ServiceId}", registration.ID);
				consulClient.Agent.ServiceDeregister(registration.ID).Wait();
			});

			return app;
		}

		private static AgentServiceRegistration CreateServiceRegistration(IConfiguration configuration)
		{
			var serviceName = configuration["ServiceName"] ?? "unknown-service";
			var serviceId = $"{serviceName}-{Guid.NewGuid()}";
			var serviceAddress = configuration["ServiceAddress"] ?? "localhost";
			var servicePort = int.Parse(configuration["ServicePort"] ?? "8080");
			var healthCheckPath = configuration["HealthCheckPath"] ?? "/health/ready";

			return new AgentServiceRegistration()
			{
				ID = serviceId,
				Name = serviceName,
				Address = serviceAddress,
				Port = servicePort,
				Tags = new[] { serviceName, "microservice" },
				Check = new AgentServiceCheck
				{
					HTTP = $"http://{serviceAddress}:{servicePort}{healthCheckPath}",
					Interval = TimeSpan.FromSeconds(10),
					Timeout = TimeSpan.FromSeconds(5),
					DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
				}
			};
		}
	}
}

