using InvoiceService.Infrastructure.Mongo;
using InvoiceService.Infrastructure.Mongo.Context;
using InvoiceService.Infrastructure.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Abstraction.Read;

namespace InvoiceSubscriber.Console.Composition
{
	public static class MongoDbRegistration
	{
		public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration cfg)
		{
			services
				.Configure<MongoDbOptions>(cfg.GetSection("MongoDb"))
				.AddSingleton<MongoContext>()
				.AddScoped<IInvoiceReadRepository, InvoiceReadRepository>();


			return services;
		}
	}
}