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
			// Check if we should skip outbox (for integration tests)
			var skipOutbox = cfg.GetValue<bool>("Testing:SkipOutbox");

			services
				.AddTransient(typeof(IInvoiceRepository), typeof(InvoiceRepository))
				.AddScoped<IEventPublisher, EfOutboxEventPublisher>()
				.AddScoped<IDateTimeProvider, SystemDateTimeProvider>()
				.RegisterDbContext(cfg, env)
				.RegisterMassTransit(cfg, skipOutbox);

			if (!skipOutbox)
				services.AddHostedService<ApplyMigrationsHostedService>();

			return services;
		}

	private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
	{
		var connectionString = cfg.GetConnectionString(ApplicationDbContext.SECTION_NAME) 
			?? throw new InvalidOperationException($"Connection string '{ApplicationDbContext.SECTION_NAME}' not found.");

		// Check if we're using SQLite for testing
		var useSqlite = cfg.GetValue<bool>("Testing:UseSqlite");

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			if (useSqlite)
			{
				// For integration tests
				options.UseSqlite(connectionString);
			}
			else
			{
				// For production
				options.UseSqlServer(
					connectionString,
					b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
			}
		});

		return services;
	}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration cfg, bool skipOutbox = false)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			cfg.GetSection("RabbitMq").Bind(rabbitMqSettings);

		services.AddMassTransit(x =>
		{
			// EF Outbox ensures atomicity between DB and message broker
			// Skip outbox for integration tests to avoid SQL Server provider conflicts
			if (!skipOutbox)
			{
				x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
				{
					o.QueryDelay = TimeSpan.FromSeconds(15);
					o.UseSqlServer();
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
				});
			}

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
