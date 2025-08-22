using DeliveryService.Application.Abstractions.Persistence;
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
	
		public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

		public Task<int> SaveChangesAsync(CancellationToken ct = default)
				=> base.SaveChangesAsync(ct);

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Shipment Aggregate
			var s = modelBuilder.Entity<Shipment>();
			s.ToTable("Shipments");
			s.HasKey(x => x.Id);
			s.Property(x => x.Status).HasConversion<int>();
			s.Property<int>("Version").IsConcurrencyToken(); // Optimistic concurrency

			// Address (Owned)
			s.OwnsOne(x => x.Address, a =>
			{
				a.Property(p => p.Street).HasMaxLength(200).IsRequired();
				a.Property(p => p.City).HasMaxLength(100).IsRequired();
				a.Property(p => p.Zip).HasMaxLength(20).IsRequired();
				a.Property(p => p.Country).HasMaxLength(2).IsRequired();
			});

			// Items (Owned Collection)
			s.OwnsMany(x => x.Items, i =>
			{
				i.ToTable("ShipmentItems");
				i.WithOwner().HasForeignKey("ShipmentId");
				i.Property<Guid>("Id");
				i.HasKey("Id");

				i.Property(x => x.ProductId).IsRequired();
				i.Property(x => x.Quantity).IsRequired();
				i.HasIndex("ShipmentId");
			});
			// Important: Tell EF to use the field, not the getter
			var nav = s.Metadata.FindNavigation(nameof(Shipment.Items));
			nav!.SetField("_items");
			nav.SetPropertyAccessMode(PropertyAccessMode.Field);


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
