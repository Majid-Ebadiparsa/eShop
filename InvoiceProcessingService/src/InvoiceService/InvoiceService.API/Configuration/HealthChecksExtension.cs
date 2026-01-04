using RabbitMQ.Client;

namespace InvoiceService.API.Configuration
{
	public static class HealthChecksExtension
	{
	public static IServiceCollection RegisterHealthChecks(this IServiceCollection services, IConfiguration cfg)
	{
		var rabbitMqConn = BuildRabbitConnectionString(cfg);
		var sqlServerConn = cfg.GetConnectionString("InvoicesDb");
		ArgumentNullException.ThrowIfNullOrEmpty(sqlServerConn);

		services.AddHealthChecks()
				.AddSqlServer(sqlServerConn, name: "sqlserver", tags: new[] { "ready" })
				.AddRabbitMQ(sp =>
				{
					var factory = new ConnectionFactory
					{
						Uri = new Uri(rabbitMqConn),
						AutomaticRecoveryEnabled = true
					};
					return factory.CreateConnectionAsync("healthcheck");
				}, name: "rabbitmq", tags: new[] { "ready" });

		return services;
	}

		private static string BuildRabbitConnectionString(IConfiguration cfg)
		{
			var cloud = cfg["CLOUDAMQP_URL"];
			if (!string.IsNullOrWhiteSpace(cloud)) return cloud;

			var host = cfg["RabbitMq:Host"] ?? "localhost";
			var vhost = cfg["RabbitMq:VirtualHost"] ?? "/";
			var user = cfg["RabbitMq:Username"] ?? "guest";
			var pass = cfg["RabbitMq:Password"] ?? "guest";
			return $"amqp://{user}:{pass}@{host}:5672/{vhost}";
		}

	}
}
