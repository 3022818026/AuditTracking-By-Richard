using AuditTracking.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


    public DbSet<CorrectiveAction> CorrectiveActions =>
    Set<CorrectiveAction>();

    public DbSet<RectificationVerification> RectificationVerifications =>
    Set<RectificationVerification>();

    public DbSet<AuditPlan> AuditPlans =>
        Set<AuditPlan>();

    public DbSet<AuditIssue> AuditIssues =>
        Set<AuditIssue>();

    public DbSet<AuditPlanOperationLog> AuditPlanOperationLogs =>
        Set<AuditPlanOperationLog>();

    public DbSet<AuditIssueOperationLog> AuditIssueOperationLogs =>
        Set<AuditIssueOperationLog>();

    public DbSet<CorrectiveActionOperationLog> CorrectiveActionOperationLogs =>
        Set<CorrectiveActionOperationLog>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RectificationVerification>(entity =>
        {
            entity.ToTable("RectificationVerifications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.VerificationNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.VerificationResult)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.VerificationComment)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.Verifier)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(x => x.DeletedBy)
                .HasMaxLength(100);

            entity.HasIndex(x => x.VerificationNo)
                .IsUnique();

            entity.HasIndex(x => x.AuditIssueId);

            entity.HasIndex(x => x.CorrectiveActionId);

            entity.HasIndex(x => x.VerificationResult);

            entity.HasIndex(x => x.VerifiedAt);

            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(x => x.AuditIssue)
                .WithMany(x => x.RectificationVerifications)
                .HasForeignKey(x => x.AuditIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CorrectiveAction)
                .WithMany(x => x.RectificationVerifications)
                .HasForeignKey(x => x.CorrectiveActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditPlan>(entity =>
        {
            entity.HasIndex(x => x.AuditNo)
                .IsUnique();

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<AuditPlanOperationLog>(entity =>
        {
            entity.HasIndex(x => x.AuditPlanId);
        });

        modelBuilder.Entity<AuditIssue>(entity =>
        {
            entity.ToTable("AuditIssues");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssueNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.IssueType)
                .HasMaxLength(50);

            entity.Property(x => x.Severity)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ResponsibleDepartment)
                .HasMaxLength(100);

            entity.Property(x => x.ResponsiblePerson)
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(x => x.DeletedBy)
                .HasMaxLength(100);

            entity.HasIndex(x => x.IssueNo)
                .IsUnique();

            entity.HasIndex(x => x.AuditPlanId);

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.DueDate);

            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(x => x.AuditPlan)
                .WithMany(x => x.AuditIssues)
                .HasForeignKey(x => x.AuditPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CorrectiveAction>(entity =>
        {
            entity.ToTable("CorrectiveActions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ActionDescription)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.ResponsibleDepartment)
                .HasMaxLength(100);

            entity.Property(x => x.ResponsiblePerson)
                .HasMaxLength(100);

            entity.Property(x => x.CompletionDescription)
                .HasMaxLength(4000);

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            entity.Property(x => x.DeletedBy)
                .HasMaxLength(100);

            entity.HasIndex(x => x.ActionNo)
                .IsUnique();

            entity.HasIndex(x => x.AuditIssueId);

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.PlannedCompletionDate);

            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(x => x.AuditIssue)
                .WithMany(x => x.CorrectiveActions)
                .HasForeignKey(x => x.AuditIssueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditIssueOperationLog>(entity =>
        {
            entity.ToTable("AuditIssueOperationLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssueNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.OperationType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.BeforeData)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.AfterData)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Operator)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Remark)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.AuditIssueId);

            entity.HasIndex(x => x.OperationType);

            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<CorrectiveActionOperationLog>(entity =>
        {
            entity.ToTable("CorrectiveActionOperationLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.OperationType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.BeforeData)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.AfterData)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Operator)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Remark)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.CorrectiveActionId);

            entity.HasIndex(x => x.OperationType);

            entity.HasIndex(x => x.CreatedAt);
        });
    }
}