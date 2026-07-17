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
}