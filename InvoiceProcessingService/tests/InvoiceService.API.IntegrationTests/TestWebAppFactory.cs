using InvoiceService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
	
namespace InvoiceService.API.IntegrationTests
{
	public class TestWebAppFactory : WebApplicationFactory<Program>
	{
		private SqliteConnection? _connection;

		protected override IHost CreateHost(IHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				// Swap DbContext to SQLite in-memory (shared connection)
				var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
				if (descriptor is not null)
					services.Remove(descriptor);

				_connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
				_connection.Open();

				services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(_connection));

				for (int i = services.Count - 1; i >= 0; i--)
				{
					var sd = services[i];
					var st = sd.ServiceType;
					var it = sd.ImplementationType;
					var nsS = st?.Namespace ?? "";
					var nsI = it?.Namespace ?? "";

					if (nsS.StartsWith("MassTransit", StringComparison.Ordinal)
							|| nsI.StartsWith("MassTransit", StringComparison.Ordinal))
					{
						services.RemoveAt(i);
					}
					else
					{
						var implTypeName = it?.FullName
								?? sd.ImplementationInstance?.GetType().FullName
								?? sd.ImplementationFactory?.GetType().FullName
								?? "";

						if (implTypeName.Contains("MassTransit", StringComparison.Ordinal))
							services.RemoveAt(i);
					}
				}

				services.AddMassTransitTestHarness(cfg =>
				{
					cfg.UsingInMemory((context, bus) => bus.ConfigureEndpoints(context));
				});

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

				// Ensure DB created (apply migrations if ModelSnapshot available)
				var sp = services.BuildServiceProvider();
				using var scope = sp.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				db.Database.EnsureCreated();
			});

			return base.CreateHost(builder);
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
