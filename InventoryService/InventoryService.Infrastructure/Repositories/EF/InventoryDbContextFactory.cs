using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InventoryService.Infrastructure.Repositories.EF
{
	public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
	{
		public InventoryDbContext CreateDbContext(string[] args)
		{
			var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "InventoryService.API");

			var configuration = new ConfigurationBuilder()
					.SetBasePath(basePath)
					.AddJsonFile("appsettings.json", optional: false)
					.AddJsonFile("appsettings.Development.json", optional: true)
					.AddEnvironmentVariables()
					.Build();

			var connectionString = configuration.GetConnectionString("InventoryDb");

			var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
			optionsBuilder.UseSqlServer(connectionString);

			return new InventoryDbContext(optionsBuilder.Options);
		}
	}
}
