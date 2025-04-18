using AutoMapper;
using Microsoft.Extensions.Logging;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Domain.ViewModels;

namespace Ziggio.Identity.Infrastructure.Services;

public class RoleGroupService : IRoleGroupService {
  private readonly IRoleGroupRepository _repository;
  private readonly IMapper _mapper;
  private readonly ILogger<UserService> _logger;

  public RoleGroupService(
    IRoleGroupRepository repository,
    IMapper mapper,
    ILogger<UserService> logger
  ) {
    _repository = repository;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task<Result<RoleGroup>> CreateRoleGroupAsync(CreateRoleGroupViewModel viewModel, CancellationToken cancellationToken) {
    var entity = new Domain.Data.Entities.RoleGroup {
      Name = viewModel.RoleGroupName,
      Roles = new List<Domain.Data.Entities.Role> { }
    };

    foreach (var role in viewModel.Roles) {
      entity.Roles.Add(_mapper.Map<Domain.Data.Entities.Role>(role));
    }

    await _repository.CreateRoleGroupAsync(entity, cancellationToken);

    return Result.Success(_mapper.Map<RoleGroup>(entity));
  }

  public async Task<Result<RoleGroup>> GetRoleGroupByIdAsync(int id, CancellationToken cancellationToken) {
    var entity = await _repository.GetRoleGroupByIdAsync(id, cancellationToken);

    if (entity is null) {
      return Result.NotFound();
    }

    return Result.Success(_mapper.Map<RoleGroup>(entity));
  }

  public async Task<Result<RoleGroup>> GetRoleGroupByNameAsync(int applicationId, string name, CancellationToken cancellationToken) {
    var entity = await _repository.GetRoleGroupByNameAsync(applicationId, name, cancellationToken);

    if (entity is null) {
      return Result.NotFound();
    }

    return Result.Success(_mapper.Map<RoleGroup>(entity));
  }
}