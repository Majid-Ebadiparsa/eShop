using Consul;
using HealthMonitorService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthMonitorService.Infrastructure.Services
{
	public class ConsulServiceDiscovery : IServiceDiscovery
	{
		private readonly IConsulClient _consulClient;
		private readonly IConfiguration _configuration;
		private readonly ILogger<ConsulServiceDiscovery> _logger;

		public ConsulServiceDiscovery(IConsulClient consulClient, IConfiguration configuration, ILogger<ConsulServiceDiscovery> logger)
		{
			_consulClient = consulClient;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<List<AgentService>> GetServicesAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var services = await _consulClient.Agent.Services(cancellationToken);
				return services.Response.Values
					.Where(s => s.Tags?.Contains("microservice") == true)
					.ToList();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to get services from Consul, using configured services");
				return GetConfiguredServices();
			}
		}

		private List<AgentService> GetConfiguredServices()
		{
			var services = new List<AgentService>();
			var serviceNames = new[] { "orderservice", "inventoryservice", "paymentservice", "deliveryservice", "invoiceservice" };

			foreach (var serviceName in serviceNames)
			{
				var baseUrl = _configuration[$"Services:{serviceName}:BaseUrl"] ?? $"http://{serviceName}:8080";
				var uri = new Uri(baseUrl);
				services.Add(new AgentService
				{
					ID = serviceName,
					Service = serviceName,
					Address = uri.Host,
					Port = uri.Port == -1 ? 8080 : uri.Port,
					Tags = new[] { "microservice" }
				});
			}

			return services;
		}
	}
}

