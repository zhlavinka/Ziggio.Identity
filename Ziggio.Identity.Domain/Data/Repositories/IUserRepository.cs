using Ziggio.Identity.Domain.Data.Entities;

namespace Ziggio.Identity.Domain.Data.Repositories;

public interface IUserRepository {
  Task<User> CheckPasswordAsync(int applicationId, string username, string password, CancellationToken cancellationToken);
  Task CreateUserAsync(User user, CancellationToken cancellationToken);
  Task DeleteUserAsync(User user, CancellationToken cancellationToken);
  Task<string> GetPasswordSaltAsync(int applicationId, string username, CancellationToken cancellationToken);
  Task<User> GetUserByEmailAsync(int applicationId, string email, CancellationToken cancellationToken);
  Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken);
  Task<User> GetUserByNameAsync(int applicationId, string username, CancellationToken cancellationToken);
  Task<List<User>> GetUsersAsync(int applicationId, CancellationToken cancellationToken);
}