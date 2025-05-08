using InventoryService.Domain.AggregatesModel;
using InventoryService.Shared;
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
			var inventoryItem = new InventoryItem(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), InventoryDefaults.DefaultProductId, 100);
			modelBuilder.Entity<InventoryItem>().HasData(inventoryItem);
		}
	}
}
