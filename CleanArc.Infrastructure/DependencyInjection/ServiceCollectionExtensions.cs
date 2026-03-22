using CleanArc.Application.Abstractions;
using CleanArc.Infrastructure.Persistence;
using CleanArc.Infrastructure.Policies;
using CleanArc.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArc.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = bool.TryParse(configuration["Storage:UseInMemory"], out var parsedUseInMemory) && parsedUseInMemory;

        if (useInMemory)
        {
            services.AddDbContext<MunicipalDbContext>(options => options.UseInMemoryDatabase("CleanArcMunicipalityDbDev"));
            services.AddScoped<IServiceRequestRepository, EfServiceRequestRepository>();
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

            services.AddDbContext<MunicipalDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IServiceRequestRepository, EfServiceRequestRepository>();
        }

        services.AddScoped<IPriorityScoringPolicy, MunicipalPriorityScoringPolicy>();
        return services;
    }
}
