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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
				.RegisterMassTransit(cfg);

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
				x.SetKebabCaseEndpointNameFormatter();

				// Consumers
				x.AddConsumer<OrderReadyToShipConsumer, OrderReadyToShipConsumerDefinition>();
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
	}
}
