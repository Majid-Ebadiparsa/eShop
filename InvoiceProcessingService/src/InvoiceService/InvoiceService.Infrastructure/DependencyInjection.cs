using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Messaging;
using InvoiceService.Infrastructure.Persistence;
using InvoiceService.Infrastructure.Persistence.Repositories;
using InvoiceService.Infrastructure.Startup;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvoiceService.Infrastructure.Configuration
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
		{
			services
				.AddTransient(typeof(IInvoiceRepository), typeof(InvoiceRepository))
				.AddScoped<IEventPublisher, EfOutboxEventPublisher>()
				.AddScoped<IDateTimeProvider, SystemDateTimeProvider>()
				.RegisterDbContext(cfg, env)
				.RegisterMassTransit(cfg)
				.AddHostedService<ApplyMigrationsHostedService>();

			return services;
		}

	private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
	{
		var connectionString = cfg.GetConnectionString(ApplicationDbContext.SECTION_NAME) 
			?? throw new InvalidOperationException($"Connection string '{ApplicationDbContext.SECTION_NAME}' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseSqlServer(
				connectionString,
				b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
		});

		return services;
	}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration cfg)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			cfg.GetSection("RabbitMq").Bind(rabbitMqSettings);

		services.AddMassTransit(x =>
		{
			// EF Outbox ensures atomicity between DB and message broker
			x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
			{
				o.QueryDelay = TimeSpan.FromSeconds(15);
				o.UseSqlServer();
				o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
			});

			x.SetKebabCaseEndpointNameFormatter();

			x.UsingRabbitMq((context, busCfg) =>
			{
				busCfg.ConfigureRabbitMqHost(cfg, rabbitMqSettings);
			});
		});

			return services;
		}
	}
}
