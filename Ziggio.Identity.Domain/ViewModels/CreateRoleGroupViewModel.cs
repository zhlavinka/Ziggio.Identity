using System.ComponentModel.DataAnnotations;
using Ziggio.Identity.Domain.Models;

namespace Ziggio.Identity.Domain.ViewModels;

public class CreateRoleGroupViewModel {
  [Required]
  public string RoleGroupName { get; set; } = "";
  public List<Role> Roles { get; set; } = new List<Role>();
}