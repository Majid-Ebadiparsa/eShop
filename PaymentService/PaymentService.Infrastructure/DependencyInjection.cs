using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PaymentService.Application.Abstractions;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Messaging.Consumer;
using PaymentService.Infrastructure.Messaging.Filters;
using PaymentService.Infrastructure.Messaging.POCO;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Persistence.Repositories;
using PaymentService.Infrastructure.Projections;
using PaymentService.Infrastructure.Psp;
using RabbitMQ.Client;

namespace PaymentService.Infrastructure
{
	public static class DependencyInjection
	{
		const string SECTION_NAME = "RabbitMq";
		const string PAYMENTDB_CONNECTION = "PaymentDb";
		const string MONGO_CONNECTION = "Mongo:Connection";
		const string QUEUE_NAME = "inventory-reserved-consumer";

		public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.RegisterDbContext(configuration)				
				.RegisterMassTransit(configuration)
				.AddHealthChecks(configuration)

				.AddScoped<IPaymentRepository, PaymentRepository>()
				.AddScoped<IPaymentReadRepository, PaymentReadRepository>()
				.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>()
				.AddSingleton<IMongoClient>(_ => new MongoClient(configuration[MONGO_CONNECTION]))
				.AddScoped<IPaymentProjectionWriter, MongoPaymentProjectionWriter>();

			services.AddScoped<IPaymentGatewayClient>(sp =>
				 {
					 var inner = new FakePaymentGatewayClient();
					 var jitter = PollyExtensions.CreateJitterRetry();
					 var breaker = PollyExtensions.CreateBreakerPolicy(configuration);

					 return new ResilientPaymentGatewayClient(inner, jitter, breaker);
				 });

			return services;
		}

		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration cfg)
		{
			services.AddDbContextFactory<PaymentDbContext>(options =>
			{
				options.UseSqlServer(
					cfg.GetConnectionString(PAYMENTDB_CONNECTION),
					b => b.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName));
			});

			services.AddDbContextFactory<PaymentDbReaderContext>(options =>
					options.UseSqlServer(cfg.GetConnectionString(PAYMENTDB_CONNECTION)));

			return services;
		}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration cfg)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			cfg.GetSection(SECTION_NAME).Bind(rabbitMqSettings);

			// Consumers register later in API (or here)
			services.AddMassTransit(x =>
			{
				x.AddConsumer<InventoryReservedConsumer>(cfg =>
				{
					cfg.UseConcurrentMessageLimit(8);
					cfg.Message<SharedService.Contracts.Events.Inventory.InventoryReserved>(m =>
					{
						// TODO: We can set retry/timeout per-message as well
					});
				});

				x.AddConsumer<PaymentProjectionConsumer>(cfg =>
				{
					cfg.UseConcurrentMessageLimit(16);
				});

				// EF Outbox ensures atomicity between DB and message broker
				x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
				{
					o.QueryDelay = TimeSpan.FromSeconds(15);
					o.UseSqlServer();
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
					o.DisableInboxCleanupService(); // If we want to add the Inbox as well, we should configure it seperately
				});

				x.SetKebabCaseEndpointNameFormatter();

				x.UsingRabbitMq((context, busCfg) =>
				{
					busCfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});

					busCfg.ReceiveEndpoint(QUEUE_NAME, e =>
					{
						// Inbox for this endpoint
						e.ConfigureConsumer<InventoryReservedConsumer>(context);
						e.UseConsumeFilter(typeof(InboxFilter<>), context, c => new InboxFilter<object>(
								context.GetRequiredService<IDbContextFactory<PaymentDbContext>>(), QUEUE_NAME));
						e.PrefetchCount = 16;
						e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2))); // retry base
					});

					busCfg.ConfigureRabbitMqHost(cfg, rabbitMqSettings);
					busCfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}

		private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration cfg)
		{
			var rabbitMqConn = BuildRabbitConnectionString(cfg);

			services
				.AddHealthChecks()
				.AddDbContextCheck<PaymentDbContext>(name: "sqlserver")
				.AddRabbitMQ(sp => {
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

			var host = cfg["RabbitMQ:Host"] ?? "localhost";
			var vhost = cfg["RabbitMQ:VirtualHost"] ?? "/";
			var user = cfg["RabbitMQ:Username"] ?? "guest";
			var pass = cfg["RabbitMQ:Password"] ?? "guest";
			return $"amqp://{user}:{pass}@{host}:5672/{vhost}";
		}
	}
}
