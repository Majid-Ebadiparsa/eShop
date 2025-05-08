using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Configuration;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Repositories.EF;

namespace OrderService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddTransient(typeof(IOrderRepository), typeof(OrderRepository))
				.RegisterDbContext(configuration)
				.RegisterMassTransit(configuration);

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
				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});
				});
			});

			services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

			return services;
		}
	}
}
