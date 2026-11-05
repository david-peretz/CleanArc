using CleanArc.Domain.Reporting;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Persistence;

public sealed class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public DbSet<RiskReportProjection> RiskReportProjections => Set<RiskReportProjection>();
    public DbSet<RiskInboxMessage> RiskInboxMessages => Set<RiskInboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var projection = modelBuilder.Entity<RiskReportProjection>();
        projection.ToTable("RiskReportProjections");
        projection.HasKey(x => x.Id);
        projection.HasIndex(x => x.SagaId).IsUnique();
        projection.Property(x => x.Status).HasMaxLength(50).IsRequired();
        projection.Property(x => x.Decision).HasMaxLength(50);
        projection.Property(x => x.Reason).HasMaxLength(1000);
        projection.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        projection.HasIndex(x => x.UpdatedAtUtc);

        var inbox = modelBuilder.Entity<RiskInboxMessage>();
        inbox.ToTable("RiskInboxMessages");
        inbox.HasKey(x => x.Id);
        inbox.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        inbox.HasIndex(x => x.ReceivedAtUtc);
        inbox.HasIndex(x => x.ProcessedAtUtc);
    }
}
