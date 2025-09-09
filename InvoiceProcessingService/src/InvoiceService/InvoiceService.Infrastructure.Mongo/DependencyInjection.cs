using InvoiceService.Infrastructure.Mongo.Context;
using InvoiceService.Infrastructure.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Abstraction.Read;

namespace InvoiceService.Infrastructure.Mongo
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMongoInfrastructure(this IServiceCollection services, IConfiguration cfg)
		{
			services.Configure<MongoDbOptions>(cfg.GetSection("MongoDb"));
			services.AddSingleton<MongoContext>();
			services.AddScoped<IInvoiceReadRepository, InvoiceReadRepository>();

			return services;
		}
	}
}
