using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Handlers;
using InventoryService.Infrastructure.Messaging;
using InventoryService.Infrastructure.Repositories;
using InventoryService.Infrastructure.Repositories.EF;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient(typeof(IInventoryRepository), typeof(InventoryRepository));

			//EF: SQLServer
			services.AddDbContext<InventoryDbContext>(options =>
				 options.UseSqlServer(
						configuration.GetConnectionString("InventoryDb"),
						b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<InventoryDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "InventoryDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);


			services.AddScoped<IOrderEventConsumer, OrderEventConsumerHandler>();

			services.AddMassTransit(x =>
			{
				x.AddConsumer<OrderCreatedEventConsumer>();

				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host("rabbitmq", "/", h =>
					{
						h.Username("guest");
						h.Password("guest");
					});

					cfg.ReceiveEndpoint("order-created-event-queue", e =>
					{
						e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
					});
				});
			});


			return services;
		}
	}
}