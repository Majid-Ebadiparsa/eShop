using InventoryService.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories.EF
{
	public class InventoryDbContext : DbContext
	{
		public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
		{
		}

		public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<InventoryItem>(item =>
			{
				item.HasKey(i => i.Id);
				item.Property(i => i.ProductId).IsRequired();
				item.Property(i => i.Quantity).IsRequired();
			});
		}
	}
}
