using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;

namespace Ziggio.Identity.Domain.Services;

public interface IUserRoleService {
  Task<Result<UserRole>> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
}