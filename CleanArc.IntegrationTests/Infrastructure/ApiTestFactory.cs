using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace CleanArc.IntegrationTests.Infrastructure;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MsSqlContainer? _sqlContainer;
    private Exception? _initException;

    public bool IsReady => _initException is null;
    public string InitError => _initException?.Message ?? string.Empty;

    public ApiTestFactory()
    {
        try
        {
            _sqlContainer = new MsSqlBuilder()
                .WithPassword("Your_strong_password123")
                .Build();
        }
        catch (Exception ex)
        {
            _initException = ex;
        }
    }

    public async Task InitializeAsync()
    {
        if (_initException is not null || _sqlContainer is null)
        {
            return;
        }

        try
        {
            await _sqlContainer.StartAsync();
        }
        catch (Exception ex)
        {
            _initException = ex;
        }
    }

    public new async Task DisposeAsync()
    {
        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_sqlContainer is null)
        {
            return;
        }

        builder.UseEnvironment("IntegrationTesting");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionString = _sqlContainer.GetConnectionString();

            var overrides = new Dictionary<string, string?>
            {
                ["Storage:UseInMemory"] = "false",
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:Issuer"] = "CleanArc.Test",
                ["Jwt:Audience"] = "CleanArc.Test.Client",
                ["Jwt:Key"] = "test-super-secret-key-1234567890",
                ["Jwt:ExpiresMinutes"] = "120"
            };

            config.AddInMemoryCollection(overrides);
        });
    }
}
