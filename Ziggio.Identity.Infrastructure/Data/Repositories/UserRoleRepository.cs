using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Entities;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Infrastructure.Data.Contexts;

namespace Ziggio.Identity.Infrastructure.Data.Repositories;

public class UserRoleRepository : IUserRoleRepository {
  private readonly IdentityDbContext _dbContext;

  public UserRoleRepository(IdentityDbContext dbContext) {
    _dbContext = dbContext;
  }

  public async Task CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken) {
    await _dbContext.AddAsync(userRole, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}