using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Configuration;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Repositories.EF;
using RabbitMQ.Client;

namespace OrderService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddTransient(typeof(IOrderRepository), typeof(OrderRepository))
				.AddScoped<IOrderEventConsumer, OrderEventConsumerHandler>()
				.RegisterDbContext(configuration)
				.RegisterMassTransit(configuration)
				.AddHealthChecks(configuration);

			return services;
		}

		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			//EF: SQLServer
			services.AddDbContext<OrderDbContext>(options =>
				 options.UseSqlServer(
						configuration.GetConnectionString("OrderDb"),
						b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<OrderDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "OrderDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

			return services;
		}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration configuration/*, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configure*/)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			services.AddMassTransit(x =>
			{
				x.AddConsumer<InventoryReservedConsumer>();
				x.AddConsumer<InventoryReservationFailedConsumer>();
				x.AddConsumer<PaymentAuthorizedConsumer>();
				x.AddConsumer<PaymentCapturedConsumer>();
				x.AddConsumer<PaymentFailedConsumer>();
				x.AddConsumer<ShipmentCreatedConsumer>();
				x.AddConsumer<ShipmentDispatchedConsumer>();
				x.AddConsumer<ShipmentDeliveredConsumer>();

				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});

					if (!string.IsNullOrWhiteSpace(rabbitMqSettings.ReceiveEndpoint))
					{
						cfg.ReceiveEndpoint(rabbitMqSettings.ReceiveEndpoint, e =>
						{
							e.ConfigureConsumer<InventoryReservedConsumer>(context);
							e.ConfigureConsumer<InventoryReservationFailedConsumer>(context);
							e.ConfigureConsumer<PaymentAuthorizedConsumer>(context);
							e.ConfigureConsumer<PaymentCapturedConsumer>(context);
							e.ConfigureConsumer<PaymentFailedConsumer>(context);
							e.ConfigureConsumer<ShipmentCreatedConsumer>(context);
							e.ConfigureConsumer<ShipmentDispatchedConsumer>(context);
							e.ConfigureConsumer<ShipmentDeliveredConsumer>(context);
						});
					}
				});
			});

			services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

			return services;
		}

		private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
		{
			var rabbitMqSettings = new RabbitMqSettings();
			configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

			var rabbitMqConn = $"amqp://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}:5672{rabbitMqSettings.VirtualHost}";

			services
				.AddHealthChecks()
				.AddDbContextCheck<OrderDbContext>(name: "sqlserver")
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
