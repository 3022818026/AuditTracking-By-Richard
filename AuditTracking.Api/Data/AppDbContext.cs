using AuditTracking.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditPlan> AuditPlans { get; set; }

    public DbSet<AuditPlanOperationLog> AuditPlanOperationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditPlan>()
            .HasIndex(x => x.AuditNo)
            .IsUnique();

        modelBuilder.Entity<AuditPlan>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<AuditPlanOperationLog>()
            .HasIndex(x => x.AuditPlanId);
    }
}