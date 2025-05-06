using InventoryService.Application.Consumers;
using MassTransit;

namespace InventoryService.API.Configuration
{
	public static class CustomMassTransitExtension
	{
		public static IServiceCollection AddCustomMassTransit(this IServiceCollection services)
		{
			_ = services.AddMassTransit(x =>
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
