using InvoiceService.Application.Abstractions.Read;
using InvoiceService.Infrastructure.Mongo.Context;
using InvoiceService.Infrastructure.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
