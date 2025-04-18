using System.ComponentModel.DataAnnotations;

namespace Ziggio.Identity.Api.ViewModels;

public class LoginViewModel {
  [Required]
  public int ApplicationId { get; set; }
  [Required]
  public string Email { get; set; }
  [Required]
  public string Password { get; set; }
  public string ReturnUrl { get; set; } = "";
}