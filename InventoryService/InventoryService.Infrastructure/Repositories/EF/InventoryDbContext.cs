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

			// Seed data for testing purposes
			modelBuilder.Entity<InventoryItem>().HasData(new InventoryItem(		
				Guid.NewGuid(),
				Guid.Parse("11111111-1111-1111-1111-111111111111"),
				100));
		}
	}
}
