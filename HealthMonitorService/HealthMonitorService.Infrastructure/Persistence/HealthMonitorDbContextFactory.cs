using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthMonitorService.Infrastructure.Persistence
{
	public class HealthMonitorDbContextFactory : IDesignTimeDbContextFactory<HealthMonitorDbContext>
	{
		public HealthMonitorDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<HealthMonitorDbContext>();
			
			// Use a temporary connection string for migrations
			// This won't actually connect, just used for generating migration scripts
			optionsBuilder.UseSqlServer("Server=localhost;Database=HealthMonitorDb;TrustServerCertificate=true;");

			return new HealthMonitorDbContext(optionsBuilder.Options);
		}
	}
}

