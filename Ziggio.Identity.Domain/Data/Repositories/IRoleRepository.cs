using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Domain.Data.Repositories;

public interface IRoleRepository {
  Task CreateRoleAsync(Role role, CancellationToken cancellationToken);
}