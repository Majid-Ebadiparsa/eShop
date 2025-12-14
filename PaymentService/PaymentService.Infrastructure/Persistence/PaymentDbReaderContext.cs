using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Persistence
{
	public class PaymentDbReaderContext : PaymentDbContext
	{
		public PaymentDbReaderContext(DbContextOptions options) : base(options) { }

		// With this method, all entities will be NoTracking by default
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		}
	}
}
