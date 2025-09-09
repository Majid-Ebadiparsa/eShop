using MongoDB.Driver;
using InvoiceService.Infrastructure.Mongo.Models;
using Microsoft.Extensions.Options;

namespace InvoiceService.Infrastructure.Mongo.Context
{
	public class MongoContext
	{
		private readonly IMongoDatabase _database;

		public MongoContext(IOptions<MongoDbOptions> options)
		{
			var client = new MongoClient(options.Value.ConnectionString);
			_database = client.GetDatabase(options.Value.DatabaseName);
		}

		public IMongoCollection<InvoiceReadModel> Invoices =>
				_database.GetCollection<InvoiceReadModel>("Invoices");
	}
}
