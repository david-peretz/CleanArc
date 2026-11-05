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
            services.AddDbContext<RiskDecisionSystemDbContext>(options => options.UseInMemoryDatabase("CleanArcInsuranceDbDev"));
            services.AddDbContext<ReportingDbContext>(options => options.UseInMemoryDatabase("CleanArcInsuranceReportDbDev"));
            services.AddScoped<IInsuranceClaimRepository, EfInsuranceClaimRepository>();
            services.AddScoped<IRiskAnalysisSagaRepository, EfRiskAnalysisSagaRepository>();
            services.AddScoped<IRiskOutboxRepository, EfRiskOutboxRepository>();
            services.AddScoped<IRiskReportReadRepository, EfRiskReportReadRepository>();
            services.AddScoped<IRiskProjectionInboxRepository, EfRiskProjectionInboxRepository>();
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
            var reportingConnectionString = configuration.GetConnectionString("ReportingConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:ReportingConnection");

            services.AddDbContext<RiskDecisionSystemDbContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<ReportingDbContext>(options => options.UseSqlServer(reportingConnectionString));
            services.AddScoped<IInsuranceClaimRepository, EfInsuranceClaimRepository>();
            services.AddScoped<IRiskAnalysisSagaRepository, EfRiskAnalysisSagaRepository>();
            services.AddScoped<IRiskOutboxRepository, EfRiskOutboxRepository>();
            services.AddScoped<IRiskReportReadRepository, EfRiskReportReadRepository>();
            services.AddScoped<IRiskProjectionInboxRepository, EfRiskProjectionInboxRepository>();
        }

        services.AddScoped<IPriorityScoringPolicy, InsuranceClaimPriorityScoringPolicy>();
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


