using RabbitMQ.Client;

namespace InvoiceService.API.Configuration
{
	public static class HealthChecksExtension
	{
		public static IServiceCollection RegisterHealthChecks(this IServiceCollection services, IConfiguration cfg)
		{
			var rabbitMqConn = cfg.GetConnectionString("RabbitMQ") ?? "amqp://localhost";
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
	}
}
