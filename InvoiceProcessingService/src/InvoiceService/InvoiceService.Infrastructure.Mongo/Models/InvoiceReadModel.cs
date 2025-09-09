using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace InvoiceService.Infrastructure.Mongo.Models
{
	public class InvoiceReadModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; } = null!;

		public string InvoiceNumber { get; set; } = null!;
		public string CustomerName { get; set; } = null!;
		public DateTime IssuedDate { get; set; }
		public double TotalAmount { get; set; }
	}
}
