using RabbitMQ.Client;

namespace InvoiceService.API.Configuration
{
	public static class HealthChecksExtension
	{
		public static IServiceCollection RegisterHealthChecks(this IServiceCollection services, IConfiguration cfg)
		{
			var rabbitMqConn = BuildRabbitConnectionString(cfg);
			var sqliteConn = cfg.GetConnectionString("InvoicesDb");
			ArgumentNullException.ThrowIfNullOrEmpty(sqliteConn);

			services.AddHealthChecks()
					.AddSqlite(sqliteConn, name: "sqlite", tags: new[] { "ready" })
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
