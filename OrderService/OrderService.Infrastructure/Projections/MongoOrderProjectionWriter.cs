using MongoDB.Driver;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.Projections
{
    public class MongoOrderProjectionWriter : IOrderProjectionWriter
    {
        private readonly IMongoCollection<OrderView> _views;

        public MongoOrderProjectionWriter(IMongoClient client)
        {
            _views = client.GetDatabase("eshop_query").GetCollection<OrderView>("orders");
        }

        public Task UpsertOrderAsync(Guid orderId, List<(Guid ProductId, int Quantity, decimal UnitPrice)> items, CancellationToken ct)
        {
            var total = items.Sum(i => i.Quantity * i.UnitPrice);
            var view = new OrderView
            {
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow,
                Total = total,
                Items = items.Select(i => new OrderItemView { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList(),
                Status = "NEW"
            };

            var update = Builders<OrderView>.Update
                .SetOnInsert(v => v.OrderId, view.OrderId)
                .Set(v => v.CreatedAt, view.CreatedAt)
                .Set(v => v.Total, view.Total)
                .Set(v => v.Items, view.Items)
                .Set(v => v.Status, view.Status);

            return _views.UpdateOneAsync(v => v.OrderId == orderId, update, new UpdateOptions { IsUpsert = true }, ct);
        }
    }
}
