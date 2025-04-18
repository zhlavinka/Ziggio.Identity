using System.Reflection;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Domain.Mapping;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Infrastructure.Data.Repositories;
using Ziggio.Identity.Infrastructure.Services;

namespace Ziggio.Identity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(IdentityDomainMappingProfile)));

        services.AddScoped<IRoleGroupRepository, RoleGroupRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRoleGroupService, RoleGroupService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserService, UserService>();
    }
}