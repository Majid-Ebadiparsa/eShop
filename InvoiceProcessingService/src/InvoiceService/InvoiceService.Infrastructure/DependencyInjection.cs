using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Messaging;
using InvoiceService.Infrastructure.Persistence;
using InvoiceService.Infrastructure.Persistence.Repositories;
using InvoiceService.Infrastructure.Startup;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
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
				.AddScoped<IEventPublisher, RabbitMqEventPublisher>()
				.AddScoped<IDateTimeProvider, SystemDateTimeProvider>()
				.RegisterDbContext(cfg, env)
				.RegisterMassTransit(cfg)
				.AddHostedService<ApplyMigrationsHostedService>();

			return services;
		}

		private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
		{
			string conn = GetConnectionString(cfg, env);

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlite(
					conn,
					b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
			});

			return services;
		}

		private static string GetConnectionString(IConfiguration cfg, IHostEnvironment env)
		{
			var cs = cfg.GetConnectionString(ApplicationDbContext.SECTION_NAME) ?? "Data Source=../data/invoices.db";
			var csb = new SqliteConnectionStringBuilder(cs);
			var dataSource = csb.DataSource;

			if (!Path.IsPathRooted(dataSource))
			{
				var baseDir = env.ContentRootPath;
				var dataDir = Path.Combine(baseDir, "data");
				Directory.CreateDirectory(dataDir);

				var fileName = Path.GetFileName(csb.DataSource);
				dataSource = Path.Combine(dataDir, fileName);
			}

			var dir = Path.GetDirectoryName(dataSource)!;
			Directory.CreateDirectory(dir);

			csb.DataSource = dataSource;
			var normalizedCs = csb.ToString();
			return normalizedCs;
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
					o.QueryDelay = TimeSpan.FromSeconds(15);
					o.UseSqlite();
					o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
				});

				x.SetKebabCaseEndpointNameFormatter();

				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
					{
						h.Username(rabbitMqSettings.Username);
						h.Password(rabbitMqSettings.Password);
					});
				});
			});



			return services;
		}
	}
}
