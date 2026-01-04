using HealthMonitorService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthMonitorService.Infrastructure.Persistence
{
	public class HealthMonitorDbContext : DbContext
	{
		public HealthMonitorDbContext(DbContextOptions<HealthMonitorDbContext> options) : base(options)
		{
		}

		public DbSet<ServiceHealthStatus> ServiceHealthStatuses => Set<ServiceHealthStatus>();
		public DbSet<ServiceHealthHistory> ServiceHealthHistories => Set<ServiceHealthHistory>();
		public DbSet<ServiceExecutionLog> ServiceExecutionLogs => Set<ServiceExecutionLog>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServiceHealthStatus>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasIndex(e => e.ServiceName).IsUnique();
				
				// Configure structured error fields as optional
				entity.Property(e => e.ErrorCode).HasMaxLength(50);
				entity.Property(e => e.ExceptionType).HasMaxLength(200);
				entity.Property(e => e.StackTrace).HasMaxLength(4000);
			});

			modelBuilder.Entity<ServiceHealthHistory>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasIndex(e => new { e.ServiceName, e.CheckedAt });
				
				// Configure structured error fields as optional
				entity.Property(e => e.ErrorCode).HasMaxLength(50);
				entity.Property(e => e.ExceptionType).HasMaxLength(200);
				entity.Property(e => e.StackTrace).HasMaxLength(4000);
			});

			modelBuilder.Entity<ServiceExecutionLog>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.HasIndex(e => new { e.ServiceName, e.ExecutionStartedAt });
				entity.HasIndex(e => e.ExecutionStartedAt);
				
				entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
				entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
				entity.Property(e => e.ErrorCode).HasMaxLength(50);
				entity.Property(e => e.ExceptionType).HasMaxLength(200);
				entity.Property(e => e.StackTrace).HasMaxLength(4000);
				entity.Property(e => e.Metadata).HasMaxLength(2000);
				entity.Property(e => e.ServiceAddress).HasMaxLength(500);
			});
		}
	}
}

