namespace InvoiceService.Infrastructure.Mongo
{
	public class MongoDbOptions
	{
		public string ConnectionString { get; set; } = null!;
		public string DatabaseName { get; set; } = null!;
	}
}
