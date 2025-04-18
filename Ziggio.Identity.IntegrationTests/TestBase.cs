using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ziggio.Identity.Api;

namespace Ziggio.Identity.IntegrationTests;

public abstract class TestBase
{
    private IdentityWebApplicationFactory<Program> _appFactory = default!;

    public void InitializeScenario()
    {
        _appFactory = new IdentityWebApplicationFactory<Program>();

        HttpClient = _appFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });
        Configuration = _appFactory.Configuration;
    }

    protected IConfiguration Configuration { get; private set; } = default!;
    protected HttpClient HttpClient { get; private set; } = default!;

    public IServiceScope CreateScope()
    {
        return _appFactory.Services.CreateScope();
    }
}
