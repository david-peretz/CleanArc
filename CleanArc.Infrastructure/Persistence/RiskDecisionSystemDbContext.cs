using CleanArc.Domain.Claims;
using CleanArc.Domain.Integration;
using CleanArc.Domain.Orchestration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArc.Infrastructure.Persistence;

public sealed class RiskDecisionSystemDbContext : IdentityDbContext<IdentityUser>
{
    public RiskDecisionSystemDbContext(DbContextOptions<RiskDecisionSystemDbContext> options)
        : base(options)
    {
    }

    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<RiskAnalysisSaga> RiskAnalysisSagas => Set<RiskAnalysisSaga>();
    public DbSet<RiskOutboxMessage> RiskOutboxMessages => Set<RiskOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var entity = modelBuilder.Entity<InsuranceClaim>();

        entity.ToTable("InsuranceClaims");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CitizenName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.AssignedDepartment).HasMaxLength(120);
        entity.Property(x => x.ClosureNotes).HasMaxLength(1000);
        entity.Property(x => x.Status).HasConversion<int>();
        entity.Property(x => x.Category).HasConversion<int>();

        entity.OwnsOne(x => x.Location, owned =>
        {
            owned.Property(p => p.Street).HasColumnName("Street").HasMaxLength(180).IsRequired();
            owned.Property(p => p.CityArea).HasColumnName("CityArea").HasMaxLength(80).IsRequired();
        });

        var saga = modelBuilder.Entity<RiskAnalysisSaga>();
        saga.ToTable("RiskAnalysisSagas");
        saga.HasKey(x => x.Id);
        saga.Property(x => x.Status).HasConversion<int>().IsRequired();
        saga.Property(x => x.Decision).HasConversion<int>();
        saga.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        saga.Property(x => x.Error).HasMaxLength(1000).IsRequired();
        saga.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        saga.HasIndex(x => x.Status);
        saga.HasIndex(x => x.CreatedAtUtc);

        var outbox = modelBuilder.Entity<RiskOutboxMessage>();
        outbox.ToTable("RiskOutboxMessages");
        outbox.HasKey(x => x.Id);
        outbox.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        outbox.Property(x => x.Payload).HasColumnType("nvarchar(max)").IsRequired();
        outbox.HasIndex(x => x.PublishedAtUtc);
        outbox.HasIndex(x => x.OccurredAtUtc);
    }
}

