using DeliveryService.Application.Repositories;
using DeliveryService.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Persistence.Repositories
{
	public sealed class ShipmentRepository : IShipmentRepository
	{
		private readonly DeliveryDbContext _db;
		public ShipmentRepository(DeliveryDbContext db) => _db = db;

		public Task AddAsync(Shipment shipment, CancellationToken ct = default)
				=> _db.Shipments.AddAsync(shipment, ct).AsTask();

		public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
				=> _db.Shipments
							.Include("_items")   // By Fluent API for items
							.FirstOrDefaultAsync(s => s.Id == id, ct);

		public Task UpdateAsync(Shipment shipment, CancellationToken ct = default)
		{
			_db.Shipments.Update(shipment);
			return Task.CompletedTask;
		}
	}
}
