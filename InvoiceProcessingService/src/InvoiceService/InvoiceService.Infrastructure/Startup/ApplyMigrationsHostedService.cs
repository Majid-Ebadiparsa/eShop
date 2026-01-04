using InvoiceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

		try
		{
			_logger.LogInformation("Applying EF Core migrations...");
			await db.Database.MigrateAsync(ct);
			_logger.LogInformation("Migrations applied successfully.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "EF Core migration failed.");
			throw;
		}
	}

		public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
	}
}
