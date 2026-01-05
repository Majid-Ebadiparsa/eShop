using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PaymentService.Infrastructure.Persistence;

public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
	public PaymentDbContext CreateDbContext(string[] args)
	{
		var apiPath = Path.GetFullPath(
				Path.Combine(Directory.GetCurrentDirectory(), "../PaymentService.API")
		);

		var configuration = new ConfigurationBuilder()
				.SetBasePath(apiPath)
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.AddEnvironmentVariables()
				.Build();

		var connectionString = configuration.GetConnectionString("PaymentDb");
		if (string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException("ConnectionString 'PaymentDb' was not found.");

		var options = new DbContextOptionsBuilder<PaymentDbContext>()
				.UseSqlServer(connectionString)
				.Options;

		return new PaymentDbContext(options);
	}
}
