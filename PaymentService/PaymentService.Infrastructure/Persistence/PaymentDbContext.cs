using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence
{
	public class PaymentDbContext : DbContext
	{
		public DbSet<Payment> Payments => Set<Payment>();

		public PaymentDbContext(DbContextOptions options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder b)
		{
			b.Entity<Payment>(e =>
			{
				e.ToTable("Payments");
				e.HasKey(x => x.Id);
				e.OwnsOne(x => x.Amount, m =>
				{
					m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
					m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(6);
				});
				e.OwnsOne(x => x.Method, m =>
				{
					m.Property(p => p.Type).HasColumnName("MethodType").HasMaxLength(32);
					m.Property(p => p.MaskedPan).HasColumnName("MaskedPan").HasMaxLength(32);
					m.Property(p => p.WalletId).HasColumnName("WalletId").HasMaxLength(64);
				});
				e.Property(x => x.OrderId).IsRequired();
				e.Property(x => x.Status).HasConversion<int>().IsRequired();
				e.OwnsMany(x => x.Attempts, m =>
				{
					m.ToTable("PaymentAttempts");
					m.WithOwner().HasForeignKey("PaymentId");
					m.Property<Guid>("Id");
					m.HasKey("Id");
					m.Property(p => p.Operation).HasMaxLength(32);
					m.Property(p => p.Succeeded);
					m.Property(p => p.CodeOrReason).HasMaxLength(256);
					m.Property(p => p.At);
				});
			});

			b.Entity<InboxMessage>(e =>
			{
				e.ToTable("InboxMessages");
				e.HasKey(x => x.Id);
				e.Property(x => x.Consumer).HasMaxLength(160).IsRequired();
				e.Property(x => x.ReceivedAt);
			});
		}
	}
}
