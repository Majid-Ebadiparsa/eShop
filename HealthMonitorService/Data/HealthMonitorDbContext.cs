using HealthMonitorService.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthMonitorService.Data
{
	public class HealthMonitorDbContext : DbContext
	{
		public HealthMonitorDbContext(DbContextOptions<HealthMonitorDbContext> options) : base(options)
		{
		}

		public DbSet<ServiceHealthStatus> ServiceHealthStatuses => Set<ServiceHealthStatus>();
		public DbSet<ServiceHealthHistory> ServiceHealthHistories => Set<ServiceHealthHistory>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServiceHealthStatus>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasIndex(e => e.ServiceName).IsUnique();
			});

			modelBuilder.Entity<ServiceHealthHistory>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasIndex(e => new { e.ServiceName, e.CheckedAt });
			});
		}
	}
}

