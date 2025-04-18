using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Domain.Data.Repositories;

public interface IUserRoleRepository {
  public Task CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
}