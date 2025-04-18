using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Ziggio.Identity.Domain;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Infrastructure.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Ziggio.Identity.Infrastructure.Services;

public class InitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public InitializationService(IServiceProvider serviceProvider)
      => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        await RegisterApplicationsAsync(scope.ServiceProvider);
        await RegisterScopesAsync(scope.ServiceProvider);
        await RegisterRolesAndUsersAsync(scope.ServiceProvider);

        async Task RegisterApplicationsAsync(IServiceProvider services)
        {
            var manager = services.GetRequiredService<IOpenIddictApplicationManager>();

            // sitebuilder - applications api
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi,
                    ClientSecret = "3f8E2u9k#z32SUXPsjUGHqBZPqFK!9B3ZbHV",
                    DisplayName = "SiteBuilder Applications API",
                    Permissions = {
                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // sitebuilder - sites api
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.SiteBuilder.SitesApi) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.SiteBuilder.SitesApi,
                    ClientSecret = "2G2N5k7z3rF@G@VpFPQ%#iw3am7zbZVxt8my",
                    DisplayName = "SiteBuilder Sites API",
                    Permissions = {
                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.SiteBuilder.SitesApi
                    },
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // refresh the test client every time
            var authorizationCodeTestClient = await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Testing.AuthorizationCodeTest);
            if (authorizationCodeTestClient is not null)
                await manager.DeleteAsync(authorizationCodeTestClient, cancellationToken);

            // authorization code test client
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Testing.AuthorizationCodeTest) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.Testing.AuthorizationCodeTest,
                    ClientSecret = "E1MJDf6vTuy1@dFnA@mnYXgDqjSSUyS7wHvfTd1Kk7Ge5",
                    DisplayName = "Ziggio Authorization Code Test Client",
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.EndSession,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Revocation,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,

                        Permissions.ResponseTypes.Code,

                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.Testing.AuthorizationCodeTest,
                        Permissions.Prefixes.Scope + "openid",
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Prefixes.Scope + "offline_access",
                    },
                    PostLogoutRedirectUris = {
                        new Uri("https://localhost:5260/signout-callback-oidc")
                    },
                    RedirectUris = {
                        new Uri("https://localhost:5260/signin-oidc")
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // refresh the test client every time
            var clientCredentialsTestClient = await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Testing.ClientCredentialsTest);
            if (clientCredentialsTestClient is not null)
                await manager.DeleteAsync(clientCredentialsTestClient, cancellationToken);

            // client credentials test client
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Testing.ClientCredentialsTest) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    
                    ClientId = Constants.Resources.Ziggio.Testing.ClientCredentialsTest,
                    ClientSecret = "xkk%sjDRCX*hCMe1HQGCCVY&%7sCcHNEz2Zy0rB8Hfj#F",
                    DisplayName = "Ziggio Client Credentials Test Client",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.ClientCredentials,

                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.Testing.ClientCredentialsTest
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // sitebuilder - .net core hosted blazor app
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.SiteBuilder.BlazorClient) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.SiteBuilder.BlazorClient,
                    ClientSecret = "V$!z%itJTzkAcwE!EEre5yXEJNWb@HzMWQbgeu",
                    DisplayName = "SiteBuilder",
                    RedirectUris = {
                        new Uri("https://localhost:5260/signin-oidc")
                    },
                    PostLogoutRedirectUris = {
                        new Uri("https://localhost:5260/signout-callback-oidc")
                    },
                    Permissions = {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.EndSession,
                        Permissions.Endpoints.Token,
                        
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        
                        Permissions.Scopes.Profile,
                        
                        Permissions.ResponseTypes.Code
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // gazoond - nutrition api
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Gazoond.NutritionApi) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.Gazoond.NutritionApi,
                    ClientSecret = "cW#5gHHeKMLcX%t7hXf8#3p#V@CS**&NH66mcc",
                    DisplayName = "Gazoond Nutrition API",
                    Permissions = {
                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.Gazoond.NutritionApi
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // gazoond - .net core hosted blazor app
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.Gazoond.BlazorClient) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.Gazoond.BlazorClient,
                    ClientSecret = "V$!z%itJTzkAcwE!EEre5yXEJNWb@HzMWQbgeu",
                    DisplayName = "Gazoond",
                    RedirectUris = {
                        new Uri("https://localhost:5260/signin-oidc")
                    },
                    PostLogoutRedirectUris = {
                        new Uri("https://localhost:5260/signout-callback-oidc")
                    },
                    Permissions = {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.EndSession,
                        Permissions.Endpoints.Token,
                        
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        
                        Permissions.Scopes.Profile,
                        
                        Permissions.ResponseTypes.Code
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            // refresh the postman client every time
            var postmanClient = await manager.FindByClientIdAsync(Constants.Resources.Ziggio.PostmanClient);
            if (postmanClient is not null)
                await manager.DeleteAsync(postmanClient, cancellationToken);

            // postman client
            if (await manager.FindByClientIdAsync(Constants.Resources.Ziggio.PostmanClient) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = Constants.Resources.Ziggio.PostmanClient,
                    ClientSecret = "42$zW%Gh1aMYkYWK&ZF7MtTgBRvdHs%gAszx",
                    DisplayName = "Ziggio Postman Client",
                    Permissions = {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.EndSession,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Revocation,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.GrantTypes.ClientCredentials,

                        Permissions.ResponseTypes.Code,

                        Permissions.Prefixes.Scope + "openid",
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Prefixes.Scope + "offline_access",

                        Permissions.Prefixes.Scope + Constants.Resources.Ziggio.SiteBuilder.SitesApi
                    },
                    PostLogoutRedirectUris = {
                        new Uri("https://localhost:5260/signout-callback-oidc"),
                        new Uri("https://auth.ziggio.local/signout-callback-oidc"),
                    },
                    RedirectUris = {
                        new Uri("https://localhost:5260/signin-oidc"),
                        new Uri("https://auth.ziggio.local/signin-oidc"),
                        new Uri("https://oauth.pstmn.io/v1/callback"),
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }
        }

        async Task RegisterScopesAsync(IServiceProvider services)
        {
            var manager = services.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync(Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi) is null)
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    DisplayName = "SiteBuilder Applications API access",
                    Name = Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi,
                    Resources = {
                        Constants.Resources.Ziggio.SiteBuilder.ApplicationsApi
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            if (await manager.FindByNameAsync(Constants.Resources.Ziggio.SiteBuilder.SitesApi) is null)
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    DisplayName = "SiteBuilder Sites API access",
                    Name = Constants.Resources.Ziggio.SiteBuilder.SitesApi,
                    Resources = {
                        Constants.Resources.Ziggio.SiteBuilder.SitesApi
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }

            if (await manager.FindByNameAsync(Constants.Resources.Ziggio.Gazoond.NutritionApi) is null)
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    DisplayName = "Gazoond Nutrition API access",
                    Name = Constants.Resources.Ziggio.Gazoond.NutritionApi,
                    Resources = {
                        Constants.Resources.Ziggio.Gazoond.NutritionApi
                    }
                };

                await manager.CreateAsync(descriptor, cancellationToken);
            }
        }

        async Task RegisterRolesAndUsersAsync(IServiceProvider services)
        {
            var roleGroupService = services.GetRequiredService<IRoleGroupService>();
            int applicationId = 0;
            int roleGroupId = 0;
            var getAppRoleGroupResult = await roleGroupService.GetRoleGroupByNameAsync(applicationId, Constants.Roles.Applications.GroupName, cancellationToken);
            RoleGroup applicationRoleGroup = null;
            if (getAppRoleGroupResult.IsSuccessful())
            {
                applicationRoleGroup = getAppRoleGroupResult.Data;
            }
            else
            {
                var createAppRoleGroupResult = await roleGroupService.CreateRoleGroupAsync(new CreateRoleGroupViewModel
                {
                    RoleGroupName = Constants.Roles.Applications.GroupName,
                    Roles = new List<Role> {
                        new Role {
                            Name = Constants.Roles.Applications.Administrator
                        },
                        new Role {
                            Name = Constants.Roles.Applications.Manager
                        },
                        new Role {
                            Name = Constants.Roles.Applications.User
                        }
                    }
                }, cancellationToken);
                if (createAppRoleGroupResult.IsSuccessful())
                {
                    applicationRoleGroup = createAppRoleGroupResult.Data;
                }
                else
                {
                    throw new Exception("Unable to create the Application role group");
                }
            }

            var getSiteRoleGroupResult = await roleGroupService.GetRoleGroupByNameAsync(applicationId, Constants.Roles.Sites.GroupName, cancellationToken);
            RoleGroup siteRoleGroup = null;
            if (getSiteRoleGroupResult.IsSuccessful())
            {
                siteRoleGroup = getSiteRoleGroupResult.Data;
            }
            else
            {
                var createSiteRoleGroupResult = await roleGroupService.CreateRoleGroupAsync(new CreateRoleGroupViewModel
                {
                    RoleGroupName = Constants.Roles.Sites.GroupName,
                    Roles = new List<Role> {
                        new Role {
                            Name = Constants.Roles.Sites.Administrator,
                        },
                        new Role {
                            Name = Constants.Roles.Sites.Manager
                        },
                        new Role {
                            Name = Constants.Roles.Sites.User
                        }
                    }
                }, cancellationToken);
                if (createSiteRoleGroupResult.IsSuccessful())
                {
                    siteRoleGroup = createSiteRoleGroupResult.Data;
                }
                else
                {
                    throw new Exception("Unable to create the Site role group");
                }
            }

            var userService = services.GetRequiredService<IUserService>();
            var adminUserEmail = "admin@zigg.io";
            var adminUserName = "administrator";
            var getUserResult = await userService.GetUserByEmailAsync(applicationId, adminUserEmail, CancellationToken.None);
            if (!getUserResult.IsSuccessful())
            {
                var appAdminRole = applicationRoleGroup.Roles.First(r => r.Name == Constants.Roles.Applications.Administrator);
                var siteAdminRole = siteRoleGroup.Roles.First(r => r.Name == Constants.Roles.Sites.Administrator);
                var createUserResult = await userService.CreateUserAsync(new CreateUserViewModel
                {
                    ApplicationId = 0,
                    Email = adminUserEmail,
                    Password = "ziggio",
                    Username = adminUserName,
                    Roles = new List<Role> {
                        appAdminRole,
                        siteAdminRole
                    }
                }, CancellationToken.None);
                if (createUserResult.HasErrors())
                {
                    throw new Exception(string.Concat(createUserResult.Errors));
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
      => Task.CompletedTask;
}
