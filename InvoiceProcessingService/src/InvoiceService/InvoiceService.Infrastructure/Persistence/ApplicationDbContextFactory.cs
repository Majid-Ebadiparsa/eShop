using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace InvoiceService.Infrastructure.Persistence
{
	public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			//if (!Debugger.IsAttached) Debugger.Launch();
			//Debugger.Break();

			var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "InvoiceService.Api"));

			var config = new ConfigurationBuilder()
					.SetBasePath(basePath)
					.AddJsonFile("appsettings.json", optional: true)
					.AddJsonFile("appsettings.Development.json", optional: true)
					.AddEnvironmentVariables()
					.Build();

			var conn = config.GetConnectionString(ApplicationDbContext.SECTION_NAME)
								?? Environment.GetEnvironmentVariable(ApplicationDbContext.SECTION_NAME);

			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			optionsBuilder.UseSqlite(conn);

			return new ApplicationDbContext(optionsBuilder.Options);
		}
	}
}
