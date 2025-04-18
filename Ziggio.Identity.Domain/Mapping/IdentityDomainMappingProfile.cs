using AutoMapper;

namespace Ziggio.Identity.Domain.Mapping;

public class IdentityDomainMappingProfile : Profile {
  public IdentityDomainMappingProfile() {
    CreateMap<Models.Role, Data.Entities.Role>().ReverseMap();
    CreateMap<Models.RoleGroup, Data.Entities.RoleGroup>().ReverseMap();
    CreateMap<Models.User, Data.Entities.User>().ReverseMap();
    CreateMap<Models.UserRole, Data.Entities.UserRole>().ReverseMap();
  }
}