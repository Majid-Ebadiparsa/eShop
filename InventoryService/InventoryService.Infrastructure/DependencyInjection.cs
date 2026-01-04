using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Configuration;
using InventoryService.Infrastructure.Handlers;
using InventoryService.Infrastructure.Messaging;
using InventoryService.Infrastructure.Repositories;
using InventoryService.Infrastructure.Repositories.EF;
using MassTransit;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace InventoryService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddTransient(typeof(IInventoryRepository), typeof(InventoryRepository))
				.RegisterDbContext(configuration)
				.RegisterMassTransit(configuration)
				.AddHealthChecks(configuration);

			services.AddScoped<IOrderEventConsumer, OrderEventConsumerHandler>();
			services.AddScoped<IEventPublisher, EfOutboxEventPublisher>();

			// MongoDB read model projection
			services.AddSingleton<IMongoClient>(_ => new MongoClient(configuration["Mongo:Connection"] ?? configuration.GetConnectionString("Mongo")));
			services.AddScoped<IInventoryProjectionWriter, Infrastructure.Projections.MongoInventoryProjectionWriter>();
			
			// MongoDB read repository for CQRS
			services.AddScoped<IInventoryReadRepository, MongoInventoryReadRepository>();

			return services;
		}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration configuration)
		{
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			services.AddMassTransit(x =>
			{
				x.AddConsumer<OrderCreatedEventConsumer>();
				x.AddConsumer<InventoryProjectionConsumer>();
				x.AddConsumer<InventoryReleaseRequestedConsumer>();

				// EF Outbox ensures atomicity between DB and message broker
				x.AddEntityFrameworkOutbox<InventoryDbContext>(o =>
				{
					o.QueryDelay = TimeSpan.FromSeconds(15);
					o.UseSqlServer();
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
					o.DisableInboxCleanupService();
				});

				x.SetKebabCaseEndpointNameFormatter();

				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});

					if (string.IsNullOrWhiteSpace(rabbitMqSettings.ReceiveEndpoint))
						throw new Exception("RabbitMQ.ReceiveEndpoint is not configured properly!");

					cfg.ReceiveEndpoint(rabbitMqSettings.ReceiveEndpoint, e =>
					{
						e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
						e.ConfigureConsumer<InventoryReleaseRequestedConsumer>(context);
							e.ConfigureConsumer<InventoryProjectionConsumer>(context);
					});
				});
			});

			return services;
		}



		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			//EF: SQLServer
			services.AddDbContext<InventoryDbContext>(options =>
				 options.UseSqlServer(
						configuration.GetConnectionString("InventoryDb"),
						b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<InventoryDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "InventoryDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

			return services;
		}

		private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
		{
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			var rabbitMqConn = $"amqp://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}:5672{rabbitMqSettings.VirtualHost}";

			services
				.AddHealthChecks()
				.AddDbContextCheck<InventoryDbContext>(name: "sqlserver")
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