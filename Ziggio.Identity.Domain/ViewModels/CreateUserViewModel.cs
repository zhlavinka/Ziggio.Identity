using System.ComponentModel.DataAnnotations;
using Ziggio.Identity.Domain.Models;

namespace Ziggio.Identity.Domain.ViewModels;

public class CreateUserViewModel {
  [Required]
  public int ApplicationId { get; set; }
  [Required]
  public string Username { get; set; }
  [Required]
  public string Password { get; set; }
  [Required]
  public string Email { get; set; }
  public string PhoneNumber { get; set; }
  public List<Role> Roles { get; set; }
}