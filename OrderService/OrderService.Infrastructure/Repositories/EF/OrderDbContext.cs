using Microsoft.EntityFrameworkCore;
using OrderService.Domain.AggregatesModel;

namespace OrderService.Infrastructure.Repositories.EF
{
	public class OrderDbContext : DbContext
	{
		public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
		{
		}

		public DbSet<Order> Orders => Set<Order>();

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
		}
	}
}	
