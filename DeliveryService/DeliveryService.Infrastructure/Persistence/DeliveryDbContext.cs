using DeliveryService.Application.Abstractions;
using DeliveryService.Domain.AggregatesModel;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Persistence
{
	public sealed class DeliveryDbContext : DbContext, IUnitOfWork
	{
		const string SCHEMA = "bus";

		public DbSet<Shipment> Shipments => Set<Shipment>();
		public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
	
		public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

		public Task<int> SaveChangesAsync(CancellationToken ct = default)
				=> base.SaveChangesAsync(ct);

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// ——— MassTransit EF Outbox/Inbox ———
			// Outbox: For storing messages before sending to the broker
			modelBuilder.AddOutboxMessageEntity();
			modelBuilder.AddOutboxStateEntity();

			// Inbox: For ensuring idempotency in Consumers (preventing duplicate processing)
			modelBuilder.AddInboxStateEntity();

			// To customize table/schema names, uncomment the following lines:
			modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", schema: SCHEMA);
			modelBuilder.Entity<OutboxState>().ToTable("OutboxState", schema: SCHEMA);
			modelBuilder.Entity<InboxState>().ToTable("InboxState", schema: SCHEMA);
		}
	}
}
