using InvoiceService.Application;
using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlite(cfg.GetConnectionString("Default"));
			});


			services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();


			services.AddMassTransit(x =>
			{
				// EF Outbox ensures atomicity between DB and message broker
				x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
				{
					o.QueryDelay = TimeSpan.FromSeconds(1);
					o.UseSqlite();
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
				});


				x.SetKebabCaseEndpointNameFormatter();


				x.UsingRabbitMq((context, cfgMq) =>
				{
					var host = cfg["RabbitMQ:Host"] ?? "localhost";
					var vhost = cfg["RabbitMQ:VirtualHost"] ?? "/";
					var user = cfg["RabbitMQ:Username"] ?? "guest";
					var pass = cfg["RabbitMQ:Password"] ?? "guest";


					cfgMq.Host(host, vhost, h =>
					{
						h.Username(user);
						h.Password(pass);
					});
				});
			});


			services.AddApplication();
			return services;
		}
	}
}
