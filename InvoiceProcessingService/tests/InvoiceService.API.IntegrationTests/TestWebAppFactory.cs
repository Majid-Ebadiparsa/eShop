using InvoiceService.API.IntegrationTests.Helpers;
using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
	
namespace InvoiceService.API.IntegrationTests
{
	public class TestWebAppFactory : WebApplicationFactory<Program>
	{
		private SqliteConnection? _connection;

		public TestWebAppFactory()
		{
			// Setup SQLite connection
			_connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
			_connection.Open();
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			// Configure settings FIRST to ensure they're available when services register
			builder.UseSetting("Testing:UseSqlite", "true");
			builder.UseSetting("Testing:SkipOutbox", "true");

			builder.ConfigureAppConfiguration((context, config) =>
			{
				// Add in-memory configuration with high priority (added AFTER other sources means it wins)
				config.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["ConnectionStrings:DefaultConnection"] = _connection!.ConnectionString,
					["Testing:UseSqlite"] = "true",
					["Testing:SkipOutbox"] = "true"
				});
			});

			builder.ConfigureServices(services =>
			{
				// Replace IEventPublisher with test implementation that doesn't use outbox
				var eventPublisherDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEventPublisher));
				if (eventPublisherDescriptor != null)
					services.Remove(eventPublisherDescriptor);
				
				services.AddScoped<IEventPublisher, TestEventPublisher>();

				// Register MassTransit test harness
				services.AddMassTransitTestHarness();

				services.PostConfigure<HealthCheckServiceOptions>(opts =>
				{
					var regs = opts.Registrations.ToList();

					var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					var unique = new List<HealthCheckRegistration>();

					for (int i = regs.Count - 1; i >= 0; i--)
					{
						var r = regs[i];
						if (seen.Add(r.Name))
							unique.Add(r);
					}

					unique.Reverse();

					opts.Registrations.Clear();
					foreach (var r in unique)
						opts.Registrations.Add(r);
				});
			});
		}

		protected override IHost CreateHost(IHostBuilder builder)
		{
			var host = base.CreateHost(builder);

			// Initialize database schema
			try
			{
				using (var scope = host.Services.CreateScope())
				{
					var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					db.Database.EnsureCreated();
				}
			}
			catch (Exception)
			{
				// If EnsureCreated fails, continue - tests may handle their own DB initialization
			}

			return host;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_connection?.Dispose();
				_connection = null;
			}
		}
	}
}
