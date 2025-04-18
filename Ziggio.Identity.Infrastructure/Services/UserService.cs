using AutoMapper;
using Microsoft.Extensions.Logging;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Infrastructure.Extensions;

namespace Ziggio.Identity.Infrastructure.Services;

public class UserService : IUserService {
  private readonly IUserRepository _userRepository;
  private readonly IPasswordService _passwordService;
  private readonly IMapper _mapper;
  private readonly ILogger<UserService> _logger;

  public UserService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IMapper mapper,
    ILogger<UserService> logger
  ) {
    _userRepository = userRepository;
    _passwordService = passwordService;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<Result> CheckPasswordAsync(int applicationId, string username, string password, CancellationToken cancellationToken) {
    var userByNameResult = await GetUserByNameAsync(applicationId, username, cancellationToken);
    if (userByNameResult.HasErrors()) {
      return Result.Error();
    }

    var salt = await _userRepository.GetPasswordSaltAsync(applicationId, username, cancellationToken);
    var hash = _passwordService.HashPassword(password, Convert.FromBase64String(salt));

    var user = await _userRepository.CheckPasswordAsync(applicationId, username, Convert.ToBase64String(hash), cancellationToken);

    if (user is null) {
      return Result.Error();
    }

    return Result.Success();
  }

  public async Task<Result<User>> CreateUserAsync(CreateUserViewModel viewModel, CancellationToken cancellationToken) {
    var userByNameResult = await _userRepository.GetUserByNameAsync(viewModel.ApplicationId, viewModel.Username, cancellationToken);
    if (userByNameResult is not null) {
      return Result.Error("User with this username already exists");
    }

    var userByEmailResult = await _userRepository.GetUserByEmailAsync(viewModel.ApplicationId, viewModel.Email, cancellationToken);
    if (userByEmailResult is not null) {
      return Result.Error("User with this email already exists");
    }

    try {
      var user = new User {
        ApplicationId = viewModel.ApplicationId,
        Email = viewModel.Email,
        PhoneNumber = viewModel.PhoneNumber,
        Username = viewModel.Username
      };

      foreach (var role in viewModel.Roles) {
        user.Roles.Add(new UserRole {
          RoleId = role.RoleId
        });
      }

      var salt = _passwordService.GenerateSalt();
      var hash = _passwordService.HashPassword(viewModel.Password, salt);

      var entity = _mapper.Map<Domain.Data.Entities.User>(user);
      entity.ConcurrencyStamp = Guid.NewGuid().ToString();
      entity.PasswordHash = Convert.ToBase64String(hash);
      entity.PasswordSalt = Convert.ToBase64String(salt);
      entity.SecurityStamp = Guid.NewGuid().ToString();

      await _userRepository.CreateUserAsync(entity, cancellationToken);

      return Result.Success(user);
    }
    catch (Exception x) {
      return Result.Error(x.Message);
    }
  }

  public async Task<Result> DeleteUserAsync(int userId, CancellationToken cancellationToken) {
    var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
    if (user is null) {
      return Result.NotFound();
    }

    await _userRepository.DeleteUserAsync(user, cancellationToken);

    return Result.Success();
  }

  public async Task<Result<User>> GetUserByEmailAsync(int applicationId, string email, CancellationToken cancellationToken) {
    var entity = await _userRepository.GetUserByEmailAsync(applicationId, email, cancellationToken);

    if (entity is null)
      return Result.NotFound();

    var model = _mapper.Map<User>(entity);

    return Result.Success(model);
  }

  public async Task<Result<User>> GetUserByIdAsync(int id, CancellationToken cancellationToken) {
    var entity = await _userRepository.GetUserByIdAsync(id, cancellationToken);

    if (entity is null)
      return Result.NotFound();

    var model = _mapper.Map<User>(entity);

    return Result.Success(model);
  }

  public async Task<Result<User>> GetUserByNameAsync(int applicationid, string username, CancellationToken cancellationToken) {
    var entity = await _userRepository.GetUserByNameAsync(applicationid, username, cancellationToken);

    if (entity is null)
      return Result.NotFound();

    var model = _mapper.Map<User>(entity);

    return Result.Success(model);
  }

  public async Task<Result<List<User>>> GetUsersAsync(int applicationId, CancellationToken cancellationToken) {
    var entities = await _userRepository.GetUsersAsync(applicationId, cancellationToken);
    var models = new List<User>();

    if (entities?.Count > 0) {
      models.AddRange(_mapper.Map<List<User>>(entities));
    }

    return Result.Success(models);
  }
}