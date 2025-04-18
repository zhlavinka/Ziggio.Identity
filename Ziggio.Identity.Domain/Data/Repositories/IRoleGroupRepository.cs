using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Domain.Data.Repositories;

public interface IRoleGroupRepository {
  Task CreateRoleGroupAsync(RoleGroup roleGroup, CancellationToken cancellationToken);
  Task<RoleGroup> GetRoleGroupByIdAsync(int id, CancellationToken cancellationToken);
  Task<RoleGroup> GetRoleGroupByNameAsync(int applicationId, string name, CancellationToken cancellationToken);
}