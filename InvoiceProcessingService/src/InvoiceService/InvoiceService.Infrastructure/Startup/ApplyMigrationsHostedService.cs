using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvoiceService.Infrastructure.Persistence;

namespace InvoiceService.Infrastructure.Startup
{
	public sealed class ApplyMigrationsHostedService : IHostedService
	{
		private readonly IServiceProvider _services;
		private readonly ILogger<ApplyMigrationsHostedService> _logger;

		public ApplyMigrationsHostedService(IServiceProvider services, ILogger<ApplyMigrationsHostedService> logger)
		{
			_services = services;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken ct)
		{
			using var scope = _services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			const int maxAttempts = 5;
			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					_logger.LogInformation("Applying EF Core migrations (attempt {Attempt}/{Max})...", attempt, maxAttempts);
					await db.Database.MigrateAsync(ct);
					_logger.LogInformation("Migrations applied successfully.");
					break;
				}
				catch (SqliteException ex) when (ex.ErrorCode == 5 /*SQLITE_BUSY*/ || ex.ErrorCode == 6 /*SQLITE_LOCKED*/)
				{
					_logger.LogWarning(ex, "SQLite is locked/busy. Retrying in 1s...");
					await Task.Delay(1000, ct);
					if (attempt == maxAttempts) throw;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "EF Core migration failed.");
					throw;
				}
			}
		}

		public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
	}
}
