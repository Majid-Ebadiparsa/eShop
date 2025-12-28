using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OrderService.Infrastructure.Repositories.EF;

namespace OrderService.Infrastructure.Design
{
    // Provides a design-time factory for EF tooling to create OrderDbContext
    // This avoids design-time resolution issues when running `dotnet ef` from the API project
    public class OrderDbContextDesignTimeFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<OrderDbContext>();

            // Prefer a connection string passed via environment variable or fallback to a sensible local default
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__OrderDb")
                       ?? "Server=localhost,1433;Database=OrderDb;User Id=sa;Password=eSh@pDem@1;TrustServerCertificate=True;";

            builder.UseSqlServer(conn, b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName));

            return new OrderDbContext(builder.Options);
        }
    }
}
