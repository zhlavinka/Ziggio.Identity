using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;

namespace Ziggio.Identity.IntegrationTests;

public class IdentityWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public IConfiguration Configuration { get; private set; } = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Testing.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = config.Build();
        });

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.Configure<OpenIddictServerOptions>(options =>
            {
                options.AccessTokenLifetime = TimeSpan.FromSeconds(5);
            });
        });
    }
}
