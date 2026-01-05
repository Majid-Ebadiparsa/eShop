using DeliveryService.Infrastructure.Persistence; // namespace DbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public sealed class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../DeliveryService.API"));

        var config = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        var connStr = config.GetConnectionString("DeliveryDb");
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("ConnectionString 'DeliveryDb' was not found.");

        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
                .UseSqlServer(connStr)
                .Options;

        return new DeliveryDbContext(options);
    }
}
