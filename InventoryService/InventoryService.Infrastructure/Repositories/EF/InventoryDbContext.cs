using InventoryService.Domain.AggregatesModel;
using InventoryService.Shared;
using Microsoft.EntityFrameworkCore;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit;

namespace InventoryService.Infrastructure.Repositories.EF
{
	public class InventoryDbContext : DbContext
	{
		public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
		{
		}

		public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

		public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<InventoryItem>(item =>
			{
				item.HasKey(i => i.Id);
				item.Property(i => i.ProductId).IsRequired();
				item.Property(i => i.Quantity).IsRequired();
			});

				// MassTransit Outbox/Inbox message entities
				modelBuilder.AddOutboxMessageEntity();
				modelBuilder.AddOutboxStateEntity();
				modelBuilder.AddInboxStateEntity();

				// Place Outbox/Inbox tables in 'bus' schema to match other services
				modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", schema: "bus");
				modelBuilder.Entity<OutboxState>().ToTable("OutboxState", schema: "bus");
				modelBuilder.Entity<InboxState>().ToTable("InboxState", schema: "bus");

				// ProcessedMessage table for consumer idempotency
				modelBuilder.Entity<ProcessedMessage>(pm =>
				{
					pm.ToTable("ProcessedMessage", schema: "bus");
					pm.HasKey(x => x.Id);
					pm.Property(x => x.MessageId).IsRequired();
					pm.Property(x => x.ConsumerName).HasMaxLength(160).IsRequired();
					pm.HasIndex(x => new { x.MessageId, x.ConsumerName }).IsUnique();
				});

			// Seed data for testing purposes
			var inventoryItem = new InventoryItem(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), InventoryDefaults.DefaultProductId, 100);
			modelBuilder.Entity<InventoryItem>().HasData(inventoryItem);
		}
	}
}
