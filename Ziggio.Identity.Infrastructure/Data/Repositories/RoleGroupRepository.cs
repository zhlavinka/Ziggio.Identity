using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Entities;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Infrastructure.Data.Contexts;

namespace Ziggio.Identity.Infrastructure.Data.Repositories;

public class RoleGroupRepository : IRoleGroupRepository {
  private readonly IdentityDbContext _dbContext;

  public RoleGroupRepository(IdentityDbContext dbContext) {
    _dbContext = dbContext;
  }

  public async Task CreateRoleGroupAsync(RoleGroup roleGroup, CancellationToken cancellationToken) {
    await _dbContext.RoleGroups.AddAsync(roleGroup, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public Task<RoleGroup> GetRoleGroupByIdAsync(int id, CancellationToken cancellationToken) {
    return _dbContext.RoleGroups
                     .Include(rg => rg.Roles)
                     .Where(r => r.RoleGroupId == id)
                     .FirstOrDefaultAsync(cancellationToken);
  }

  public Task<RoleGroup> GetRoleGroupByNameAsync(int applicationId, string name, CancellationToken cancellationToken) {
    return _dbContext.RoleGroups
                     .Include(rg => rg.Roles)
                     .Where(r => r.ApplicationId == applicationId && r.Name == name)
                     .FirstOrDefaultAsync(cancellationToken);
  }
}