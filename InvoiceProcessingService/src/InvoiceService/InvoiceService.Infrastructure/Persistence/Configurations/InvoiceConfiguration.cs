using InvoiceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceService.Infrastructure.Persistence.Configurations
{
	public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
	{
		public void Configure(EntityTypeBuilder<Invoice> b)
		{
			b.ToTable("Invoices");
			b.HasKey(x => x.Id);

			b.Property(x => x.Description).HasMaxLength(500).IsRequired();
			b.Property(x => x.Supplier).HasMaxLength(200).IsRequired();
			b.Property(x => x.DueDate).IsRequired();

			b.HasMany(i => i.Lines)
			 .WithOne(l => l.Invoice)
			 .HasForeignKey(l => l.InvoiceId)
			 .OnDelete(DeleteBehavior.Cascade);

			var nav = b.Metadata.FindNavigation(nameof(Invoice.Lines))!;
			nav.SetField("_lines");
			nav.SetPropertyAccessMode(PropertyAccessMode.Field);
		}
	}
}
