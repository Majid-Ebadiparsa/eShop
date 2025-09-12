using PaymentService.Application.Abstractions;
using MongoDB.Driver;

namespace PaymentService.Infrastructure.Projections
{
	public class MongoPaymentProjectionWriter : IPaymentProjectionWriter
	{
		private readonly IMongoCollection<PaymentView> _views;
		public MongoPaymentProjectionWriter(IMongoClient client)
		{
			_views = client.GetDatabase("eshop_query").GetCollection<PaymentView>("payments");
		}

		public Task UpsertAuthorizedAsync(Guid paymentId, Guid orderId, CancellationToken ct)
				=> _views.UpdateOneAsync(v => v.PaymentId == paymentId,
																 Builders<PaymentView>.Update
																		 .SetOnInsert(v => v.PaymentId, paymentId)
																		 .Set(v => v.OrderId, orderId)
																		 .Set(v => v.Status, "AUTHORIZED"),
																 new UpdateOptions { IsUpsert = true }, ct);

		public Task SetCapturedAsync(Guid paymentId, string captureId, CancellationToken ct)
				=> _views.UpdateOneAsync(v => v.PaymentId == paymentId,
																 Builders<PaymentView>.Update
																		 .Set(v => v.Status, "CAPTURED")
																		 .Set(v => v.CaptureId, captureId),
																 new UpdateOptions { IsUpsert = true }, ct);

		public Task SetFailedAsync(Guid orderId, string reason, CancellationToken ct)
				=> _views.UpdateOneAsync(v => v.OrderId == orderId,
																 Builders<PaymentView>.Update
																		 .Set(v => v.Status, "FAILED")
																		 .Set(v => v.Reason, reason),
																 new UpdateOptions { IsUpsert = true }, ct);
	}
}
