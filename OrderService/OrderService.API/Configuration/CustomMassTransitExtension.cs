using MassTransit;

namespace OrderService.API.Configuration
{
	public static class CustomMassTransitExtension
	{
		public static IServiceCollection AddCustomMassTransit(this IServiceCollection services)
		{
			_ = services.AddMassTransit(x =>
			{
				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host("rabbitmq", "/", h =>
					{
						h.Username("guest");
						h.Password("guest");
					});
				});
			});
			return services;
		}
	}
}
