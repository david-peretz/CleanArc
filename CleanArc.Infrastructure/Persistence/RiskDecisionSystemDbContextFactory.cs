using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArc.Infrastructure.Persistence;

public sealed class RiskDecisionSystemDbContextFactory : IDesignTimeDbContextFactory<RiskDecisionSystemDbContext>
{
    public RiskDecisionSystemDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RiskDecisionSystemDbContext>();

        var connectionString =
            "Server=localhost,1433;Database=CleanArcMunicipalityDbDesign;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);
        return new RiskDecisionSystemDbContext(optionsBuilder.Options);
    }
}
