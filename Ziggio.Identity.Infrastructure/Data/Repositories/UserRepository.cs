using Microsoft.EntityFrameworkCore;
using Ziggio.Identity.Domain.Data.Entities;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Infrastructure.Data.Contexts;

namespace Ziggio.Identity.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository {
  private readonly IdentityDbContext _dbContext;

  public UserRepository(IdentityDbContext dbContext) {
    _dbContext = dbContext;
  }

  public Task<User> CheckPasswordAsync(int applicationId, string username, string password, CancellationToken cancellationToken) {
    return _dbContext.Users.FirstOrDefaultAsync(u => u.ApplicationId == applicationId && u.Username == username && u.PasswordHash == password, cancellationToken);
  }

  public async Task CreateUserAsync(User user, CancellationToken cancellationToken) {
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task DeleteUserAsync(User user, CancellationToken cancellationToken) {
    _dbContext.Users.Remove(user);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<string> GetPasswordSaltAsync(int applicationId, string username, CancellationToken cancellationToken) {
    return (await _dbContext.Users.FirstAsync(u => u.ApplicationId == applicationId && u.Username == username, cancellationToken)).PasswordSalt;
  }

  public Task<User> GetUserByEmailAsync(int applicationId, string email, CancellationToken cancellationToken) {
    return _dbContext.Users.FirstOrDefaultAsync(u => u.ApplicationId == applicationId && u.Email == email, cancellationToken);
  }

  public Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken) {
    return _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
  }

  public Task<User> GetUserByNameAsync(int applicationId, string username, CancellationToken cancellationToken) {
    return _dbContext.Users.FirstOrDefaultAsync(u => u.ApplicationId == applicationId && u.Username == username, cancellationToken);
  }

  public Task<List<User>> GetUsersAsync(int applicatonId, CancellationToken cancellationToken) {
    return _dbContext.Users.Where(u => u.ApplicationId == applicatonId).ToListAsync(cancellationToken);
  }
}