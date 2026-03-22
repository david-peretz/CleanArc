using CleanArc.IntegrationTests.Infrastructure;
using CleanArc.WebApi.Contracts;
using CleanArc.WebApi.Contracts.Auth;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CleanArc.IntegrationTests;

public sealed class ServiceRequestsApiTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ServiceRequestsApiTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Manager_Can_Login_Create_And_View_Open_Requests()
    {
        if (!_factory.IsReady)
        {
            return;
        }

        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("manager", "Manager123!"));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login!.AccessToken));

        var createResponse = await client.PostAsJsonAsync("/api/service-requests", new CreateServiceRequestRequest(
            "Neta Cohen",
            "Street light is flickering for two days near playground crossing.",
            3,
            "Rokach 12",
            "North",
            true));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var openResponse = await client.GetAsync("/api/service-requests/open");
        openResponse.EnsureSuccessStatusCode();

        var list = await openResponse.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list!);
    }
}
