using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceService.API.IntegrationTests
{
	public class CustomWebAppFactory : WebApplicationFactory<Program>
	{
		private SqliteConnection? _conn;

		protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");

			builder.ConfigureServices(services =>
			{
				// 1) DbContext → SQLite InMemory
				_conn = new SqliteConnection("DataSource=:memory:");
				_conn.Open();

				var dbDesc = services.Single(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
				services.Remove(dbDesc);
				services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(_conn));

				// Ensure schema
				using var sp = services.BuildServiceProvider();
				using var scope = sp.CreateScope();
				scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();

				// 2) Replace IEventPublisher with Test Double
				var epDesc = services.FirstOrDefault(d => d.ServiceType == typeof(IEventPublisher));
				if (epDesc != null) services.Remove(epDesc);
				services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
			});
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_conn?.Dispose();
		}
	}
}
