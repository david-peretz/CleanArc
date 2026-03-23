using CleanArc.Domain.Claims;
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
    }
}

