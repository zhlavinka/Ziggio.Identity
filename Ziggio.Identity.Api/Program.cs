using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using Ziggio.Identity.Api.Extensions;
using Ziggio.Identity.Domain;
using Ziggio.Identity.Infrastructure.Data.Contexts;
using Ziggio.Identity.Infrastructure.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Ziggio.Identity.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // configure serilog from appsettings.json
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build())
            .CreateLogger();

        try
        {
            Log.Information("Starting Ziggio Identity API...");
            var builder = WebApplication.CreateBuilder(args);

            // use serilog as logging provider
            builder.Host.UseSerilog();

            var configuration = builder.Configuration;

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // allows challenge responses for token-based APIs
                options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/account/login";
            });

            builder.Services.AddAuthorization(options =>
            {
                //var serverKeyPolicy = new AuthorizationPolicyBuilder(ServerKeyAuthenticationSchemeOptions.SchemeName)
                //  .RequireAuthenticatedUser()
                //  .Build();

                //options.AddPolicy(ServerKeyAuthenticationSchemeOptions.SchemeName, serverKeyPolicy);
            });

            builder.Services.AddDbContext<IdentityDbContext>(options => {
                options.UseNpgsql(configuration.GetConnectionString("IdentityDb"));

                // Register the entity sets needed by OpenIddict.
                options.UseOpenIddict();

            }, ServiceLifetime.Transient);

            builder.Services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options => {
                    // Configure OpenIddict to use the EF Core stores/models.
                    options.UseEntityFrameworkCore()
                       .UseDbContext<IdentityDbContext>();
                })
                // Register the OpenIddict server components.
                .AddServer(options => {
                    options
                        .SetAccessTokenLifetime(TimeSpan.FromMinutes(15))
                        .SetRefreshTokenLifetime(TimeSpan.FromDays(30))

                        .AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .RequireProofKeyForCodeExchange()
                        .AllowRefreshTokenFlow()
                            .DisableSlidingRefreshTokenExpiration()
                            .SetRefreshTokenReuseLeeway(
                                builder.Environment.IsTesting()
                                    ? TimeSpan.FromSeconds(0)
                                    : TimeSpan.FromSeconds(15));

                    // reference refresh tokens for revokability
                    options.UseReferenceRefreshTokens();

                    options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetEndSessionEndpointUris("/connect/logout")
                        .SetRevocationEndpointUris("/connect/revoke")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserInfoEndpointUris("/connect/userinfo");

                    // Encryption and signing of tokens
                    //options.AddEphemeralEncryptionKey()
                    //       .AddEphemeralSigningKey();
                    options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                    options.DisableAccessTokenEncryption();
                    options.AddDevelopmentSigningCertificate();

                    // Register scopes (permissions)
                    options.RegisterScopes(
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                        Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi,
                        Constants.Resources.Ziggio.SiteBuilder.SitesApi,
                        Constants.Resources.Ziggio.Testing.AuthorizationCodeTest,
                        Constants.Resources.Ziggio.Testing.ClientCredentialsTest);

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough();
                })
                .AddValidation(builder => {
                    builder.Configure(options => {
                        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="));
                    });

                    // Import the configuration from the local OpenIddict server instance.
                    builder.UseLocalServer();

                    // Register the ASP.NET Core host.
                    builder.UseAspNetCore();
                });

            builder.Services.AddHttpContextAccessor();

            builder.Services.RegisterServices();

            builder.Services.AddHostedService<InitializationService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception x)
        {
            Log.Fatal(x, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
