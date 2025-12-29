using InvoiceService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Infrastructure.Persistence
{
	public class ApplicationDbContext : DbContext
	{
		public const string SECTION_NAME = "InvoicesDb";

		public DbSet<Invoice> Invoices => Set<Invoice>();
		public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

		public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

			modelBuilder.AddInboxStateEntity();
			modelBuilder.AddOutboxMessageEntity();
			modelBuilder.AddOutboxStateEntity();

			// ProcessedMessage table for consumer idempotency
			modelBuilder.Entity<ProcessedMessage>(pm =>
			{
				pm.ToTable("ProcessedMessage", schema: "bus");
				pm.HasKey(x => x.Id);
				pm.Property(x => x.MessageId).IsRequired();
				pm.Property(x => x.ConsumerName).HasMaxLength(160).IsRequired();
				pm.HasIndex(x => new { x.MessageId, x.ConsumerName }).IsUnique();
			});

			base.OnModelCreating(modelBuilder);
		}
	}
}
