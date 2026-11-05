using CleanArc.Application.Abstractions;
using CleanArc.Application.Services;
using CleanArc.Application.Orchestration.Handlers;
using CleanArc.Application.Reporting.Handlers;
using CleanArc.Infrastructure.DependencyInjection;
using CleanArc.Infrastructure.Messaging;
using CleanArc.Infrastructure.Persistence;
using CleanArc.WebApi.HostedServices;
using CleanArc.WebApi.Middlewares;
using CleanArc.WebApi.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<InsuranceClaimService>();
builder.Services.AddScoped<InsuranceRiskAnalysisService>();
builder.Services.AddScoped<EnqueueRiskAnalysisSagaHandler>();
builder.Services.AddScoped<GetRiskAnalysisSagaHandler>();
builder.Services.AddScoped<ProcessRiskAnalysisSagaHandler>();
builder.Services.AddScoped<GetLargeRiskReportQueryHandler>();
builder.Services.AddSingleton<IIntegrationEventQueue, InMemoryIntegrationEventQueue>();
builder.Services.AddHostedService<RiskSagaWorker>();
builder.Services.AddHostedService<RiskOutboxPublisherWorker>();
builder.Services.AddHostedService<RiskProjectionConsumerWorker>();

builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<RiskDecisionSystemDbContext>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<JwtTokenService>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RiskDecisionSystemDbContext>();
    var useInMemory = app.Configuration.GetValue<bool>("Storage:UseInMemory");

    if (!useInMemory)
    {
        await db.Database.MigrateAsync();
    }
    var reportDb = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    await reportDb.Database.EnsureCreatedAsync();

    await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseMiddleware<ExceptionMappingMiddleware>();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;

