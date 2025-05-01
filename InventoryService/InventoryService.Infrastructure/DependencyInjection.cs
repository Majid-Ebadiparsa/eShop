using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Repositories;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient(typeof(IInventoryRepository), typeof(InventoryRepository));

			//EF: SQLServer
			services.AddDbContext<InventoryDbContext>(options =>
				 options.UseSqlServer(
						configuration.GetConnectionString("InventoryDb"),
						b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<InventoryDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "InventoryDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

			return services;
		}
	}
}