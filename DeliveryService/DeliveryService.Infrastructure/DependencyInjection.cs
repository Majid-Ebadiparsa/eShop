using DeliveryService.Application.Abstractions.Messaging;
using DeliveryService.Application.Abstractions.Persistence;
using DeliveryService.Application.Abstractions.Services;
using DeliveryService.Infrastructure.Configuration;
using DeliveryService.Infrastructure.Messaging;
using DeliveryService.Infrastructure.Messaging.Consumers;
using DeliveryService.Infrastructure.Messaging.Consumers.Definitions;
using DeliveryService.Infrastructure.Persistence;
using DeliveryService.Infrastructure.Persistence.Repositories;
using DeliveryService.Infrastructure.Services;
using MassTransit;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace DeliveryService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
		{
			services
				.RegisterDbContext(cfg)
				.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DeliveryDbContext>())
				.AddScoped<IShipmentRepository, ShipmentRepository>()
				.AddScoped<IEventPublisher, EfOutboxEventPublisher>()
				.AddScoped<IShipmentService, ShipmentService>()
				.AddScoped<ICarrierClient, MockCarrierClient>()
				.RegisterOrderServiceClient(cfg)
				.RegisterMassTransit(cfg)
				.RegisterMongoProjections(cfg)
				.AddHealthChecks(cfg);

			return services;
		}

	// Register Mongo and projections
	private static IServiceCollection RegisterMongoProjections(this IServiceCollection services, IConfiguration cfg)
	{
		services.AddSingleton<IMongoClient>(_ => new MongoClient(cfg["Mongo:Connection"] ?? cfg.GetConnectionString("Mongo")));
		services.AddScoped<DeliveryService.Application.Abstractions.IShipmentProjectionWriter, DeliveryService.Infrastructure.Projections.MongoShipmentProjectionWriter>();
		services.AddScoped<ShipmentProjectionConsumer>();
		
		// MongoDB read repository for CQRS
		services.AddScoped<IShipmentReadRepository, MongoShipmentReadRepository>();
		
		return services;
	}

		private static IServiceCollection RegisterOrderServiceClient(this IServiceCollection services, IConfiguration cfg)
		{
			var orderServiceUrl = cfg["OrderService:BaseUrl"] ?? "http://orderservice:8080";
			services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
			{
				client.BaseAddress = new Uri(orderServiceUrl);
				client.Timeout = TimeSpan.FromSeconds(30);
			});
			return services;
		}

		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			//EF: SQLServer
			services.AddDbContext<DeliveryDbContext>(options =>
				 options.UseSqlServer(
						configuration.GetConnectionString("DeliveryDb"),
						b => b.MigrationsAssembly(typeof(DeliveryDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<DeliveryDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "DeliveryDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

			return services;
		}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration configuration/*, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configure*/)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			services.AddMassTransit(x =>
			{
				x.AddConsumer<ShipmentProjectionConsumer>();
				x.SetKebabCaseEndpointNameFormatter();

				// Consumers
				x.AddConsumer<OrderReadyToShipConsumer, OrderReadyToShipConsumerDefinition>();
				x.AddConsumer<PaymentCapturedConsumer, PaymentCapturedConsumerDefinition>();
				x.AddConsumer<OnShipmentCreatedConsumer, OnShipmentCreatedConsumerDefinition>();

				// Outbox EF (Recommended)
				x.AddEntityFrameworkOutbox<DeliveryDbContext>(o =>
				{
					o.UseSqlServer();
					o.QueryDelay = TimeSpan.FromSeconds(1);
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(1);
					// o.UseBusOutbox(); // Optional: Use the bus outbox instead of the EF outbox. It is not durable, but faster and keeps in the pipeline's memory
				});

				x.UsingRabbitMq((context, cfgBus) =>
				{
					cfgBus.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});

					// Activate the Outbox in the bus
					cfgBus.ConfigureEndpoints(context);
				});
			});

			return services;
		}

		private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
		{
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			var rabbitMqConn = $"amqp://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}:5672{rabbitMqSettings.VirtualHost}";

			services
				.AddHealthChecks()
				.AddDbContextCheck<DeliveryDbContext>(name: "sqlserver")
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
