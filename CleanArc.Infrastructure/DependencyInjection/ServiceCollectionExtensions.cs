using CleanArc.Application.Abstractions;
using CleanArc.Application.Interfaces;
using CleanArc.Infrastructure.AiClient;
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
            services.AddDbContext<RiskDecisionSystemDbContext>(options => options.UseInMemoryDatabase("CleanArcMunicipalityDbDev"));
            services.AddScoped<IServiceRequestRepository, EfServiceRequestRepository>();
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

            services.AddDbContext<RiskDecisionSystemDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IServiceRequestRepository, EfServiceRequestRepository>();
        }

        services.AddScoped<IPriorityScoringPolicy, MunicipalPriorityScoringPolicy>();
        services.Configure<PythonRiskServiceOptions>(configuration.GetSection(PythonRiskServiceOptions.SectionName));
        services.AddHttpClient<IPythonRiskAgentClient, PythonRiskAgentClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<PythonRiskServiceOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return services;
    }
}
