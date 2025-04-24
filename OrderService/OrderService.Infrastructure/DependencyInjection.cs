using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Repositories.EF;

namespace OrderService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient(typeof(IOrderRepository), typeof(OrderRepository));

			//EF: SQLServer
			services.AddDbContext<OrderDbContext>(options =>
				 options.UseSqlServer(
					  configuration.GetConnectionString("OrderDb"),
					  b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

			//EF: InMemory
			//services.AddDbContext<OrderDbContext>(options =>
			//	 options.UseInMemoryDatabase(databaseName: "OrderDB"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

			return services;
		}
	}
}
