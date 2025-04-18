using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;

namespace Ziggio.Identity.Domain.Services;

public interface IUserService {
  Task<Result> CheckPasswordAsync(int applicationId, string username, string password, CancellationToken cancellationToken);
  Task<Result<User>> CreateUserAsync(CreateUserViewModel viewModel, CancellationToken cancellationToken);
  Task<Result> DeleteUserAsync(int userId, CancellationToken cancellationToken);
  Task<Result<User>> GetUserByEmailAsync(int applicationId, string email, CancellationToken cancellationToken);
  Task<Result<User>> GetUserByIdAsync(int id, CancellationToken cancellationToken);
  Task<Result<User>> GetUserByNameAsync(int applicationId, string username, CancellationToken cancellationToken);
  Task<Result<List<User>>> GetUsersAsync(int applicationId, CancellationToken cancellationToken);
}