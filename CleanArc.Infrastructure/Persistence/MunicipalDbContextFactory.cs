using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArc.Infrastructure.Persistence;

public sealed class MunicipalDbContextFactory : IDesignTimeDbContextFactory<MunicipalDbContext>
{
    public MunicipalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MunicipalDbContext>();

        var connectionString =
            "Server=localhost,1433;Database=CleanArcMunicipalityDbDesign;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);
        return new MunicipalDbContext(optionsBuilder.Options);
    }
}
