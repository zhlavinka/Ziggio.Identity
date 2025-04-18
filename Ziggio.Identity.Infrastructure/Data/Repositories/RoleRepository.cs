using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Entities;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Infrastructure.Data.Contexts;

namespace Ziggio.Identity.Infrastructure.Data.Repositories;

public class RoleRepository : IRoleRepository {
  private readonly IdentityDbContext _dbContext;

  public RoleRepository(IdentityDbContext dbContext) {
    _dbContext = dbContext;
  }

  public async Task CreateRoleAsync(Role role, CancellationToken cancellationToken) {
    await _dbContext.Roles.AddAsync(role, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}