using AutoMapper;
using Microsoft.Extensions.Logging;
using Ziggio.Identity.Domain.Data.Repositories;
using Ziggio.Identity.Domain.Models;
using Ziggio.Identity.Domain.Results;
using Ziggio.Identity.Domain.Services;
using Ziggio.Identity.Domain.ViewModels;
using Ziggio.Identity.Infrastructure.Extensions;

namespace Ziggio.Identity.Infrastructure.Services;

public class RoleService : IRoleService {
  private readonly IRoleRepository _roleRepository;
  private readonly IMapper _mapper;
  private readonly ILogger<RoleService> _logger;

  public RoleService(
    IRoleRepository roleRepository,
    IMapper mapper,
    ILogger<RoleService> logger
  ) {
    _roleRepository = roleRepository;
    _mapper = mapper;
    _logger = logger;
  }
}