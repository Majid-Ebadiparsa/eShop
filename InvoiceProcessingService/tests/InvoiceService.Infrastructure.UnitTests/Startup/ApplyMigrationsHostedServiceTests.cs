using FluentAssertions;
using InvoiceService.Infrastructure.Persistence;
using InvoiceService.Infrastructure.Startup;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace InvoiceService.Infrastructure.UnitTests.Startup
{
	public class ApplyMigrationsHostedServiceTests
	{
		[Fact]
		public async Task StartAsync_Should_Create_All_Tables_On_Fresh_Database()
		{
			// Use a real temporary file to persist the SQLite DB during the test
			var dbFile = Path.Combine(Path.GetTempPath(), $"invoices_test_{Guid.NewGuid():N}.db");
			var connString = $"Data Source={dbFile}";

			var services = new ServiceCollection();
			services.AddLogging();
			services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(connString));

			var sp = services.BuildServiceProvider();
			var logger = new NullLogger<ApplyMigrationsHostedService>();
			var hosted = new ApplyMigrationsHostedService(sp, logger);

			try
			{
				await hosted.StartAsync(CancellationToken.None);

				// Verify expected tables exist
				using var conn = new SqliteConnection(connString);
				await conn.OpenAsync();

				var expectedTables = new[]
				{
								"Invoices",
								"InvoiceLines",
                // MassTransit EF Inbox/Outbox artifacts (names are conventional)
                "InboxState",
								"OutboxMessage",
								"OutboxState"
						};

				foreach (var table in expectedTables)
				{
					using var cmd = conn.CreateCommand();
					cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tname";
					cmd.Parameters.Add(new SqliteParameter("@tname", table));

					var result = await cmd.ExecuteScalarAsync();
					result.Should().NotBeNull($"table '{table}' should exist after migrations");
				}
			}
			finally
			{
				try { File.Delete(dbFile); } catch { /* ignore */ }
			}
		}
	}
}
