using InvoiceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceService.Infrastructure.Persistence.Configurations
{
	public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
	{
		public void Configure(EntityTypeBuilder<InvoiceLine> b)
		{
			b.ToTable("InvoiceLines");
			b.HasKey(x => x.Id);
			b.Property(x => x.Description).HasMaxLength(500).IsRequired();
			b.Property(x => x.Price).IsRequired();
			b.Property(x => x.Quantity).IsRequired();
			b.Property<Guid>("InvoiceId");
			b.HasIndex("InvoiceId");
		}
	}
}
