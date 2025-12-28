using Microsoft.EntityFrameworkCore;
using OrderService.Domain.AggregatesModel;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit;

namespace OrderService.Infrastructure.Repositories.EF
{
	// Note: add MassTransit Outbox/Inbox entities so EF migrations can create the Outbox tables
}

namespace OrderService.Infrastructure.Repositories.EF
{
	public class OrderDbContext : DbContext
	{
		public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
		{
		}

		public DbSet<Order> Orders => Set<Order>();
	public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Order>(order =>
			{
				order.HasKey(o => o.Id);
				order.OwnsOne(o => o.ShippingAddress); // Because Address is a ValueObject

				order.HasMany(o => o.Items)
					  .WithOne()
					  .HasForeignKey("OrderId");
			});

			modelBuilder.Entity<OrderItem>().HasKey(o => o.Id);

			// MassTransit EF Outbox/Inbox entities (store outgoing messages durably before publishing)
			modelBuilder.AddOutboxMessageEntity();
			modelBuilder.AddOutboxStateEntity();
			modelBuilder.AddInboxStateEntity();

			// Place Outbox/Inbox tables in 'bus' schema to match DeliveryService convention
			modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", schema: "bus");
			modelBuilder.Entity<OutboxState>().ToTable("OutboxState", schema: "bus");
			modelBuilder.Entity<InboxState>().ToTable("InboxState", schema: "bus");

			// ProcessedMessage table for consumer idempotency
			modelBuilder.Entity<ProcessedMessage>(pm =>
			{
				pm.ToTable("ProcessedMessage", schema: "bus");
				pm.HasKey(x => x.Id);
				pm.HasIndex(x => new { x.MessageId, x.ConsumerName }).IsUnique();
				pm.Property(x => x.ProcessedAt).IsRequired();
			});
		}
	}
}	
