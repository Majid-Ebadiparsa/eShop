using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Messaging;
using InvoiceService.Infrastructure.Persistence;
using InvoiceService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
		{
			services
				.AddTransient(typeof(IInvoiceRepository), typeof(InvoiceRepository))
				.AddScoped<IEventPublisher, RabbitMqEventPublisher>()
				.AddScoped<IDateTimeProvider, SystemDateTimeProvider>()
				.RegisterDbContext(cfg)
				.RegisterMassTransit(cfg);

			return services;
		}

		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration cfg)
		{
			var cs = cfg.GetConnectionString(ApplicationDbContext.SECTION_NAME);
			Directory.CreateDirectory(Path.GetDirectoryName(new SqliteConnectionStringBuilder(cs).DataSource)!);

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlite(
					cfg.GetConnectionString(ApplicationDbContext.SECTION_NAME),
					b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
			});

			return services;
		}

		private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration cfg)
		{
			// Bind RabbitMQ settings
			var rabbitMqSettings = new RabbitMqSettings();
			cfg.GetSection("RabbitMQ").Bind(rabbitMqSettings);

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
			});



			return services;
		}
	}
}
