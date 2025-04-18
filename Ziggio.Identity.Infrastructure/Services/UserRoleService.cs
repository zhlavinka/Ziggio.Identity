using AutoMapper;
using Microsoft.Extensions.Logging;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Infrastructure.Extensions;

namespace Ziggio.Identity.Infrastructure.Services;

public class UserRoleService : IUserRoleService {
  private readonly IUserRoleRepository _repository;
  private readonly IMapper _mapper;
  private readonly ILogger<UserService> _logger;

  public UserRoleService(
    IUserRoleRepository repository,
    IMapper mapper,
    ILogger<UserService> logger
  ) {
    _repository = repository;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<Result<UserRole>> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken) {
    var entity = _mapper.Map<Domain.Data.Entities.UserRole>(userRole);

    await _repository.CreateUserRoleAsync(entity, cancellationToken);

    return Result.Success(userRole);
  }
}