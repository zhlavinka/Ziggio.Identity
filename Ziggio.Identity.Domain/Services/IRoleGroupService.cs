using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;

namespace Ziggio.Identity.Domain.Services;

public interface IRoleGroupService {
  Task<Result<RoleGroup>> CreateRoleGroupAsync(CreateRoleGroupViewModel viewModel, CancellationToken cancellationToken);
  Task<Result<RoleGroup>> GetRoleGroupByIdAsync(int id, CancellationToken cancellationToken);
  Task<Result<RoleGroup>> GetRoleGroupByNameAsync(int applicationId, string name, CancellationToken cancellationToken);
}