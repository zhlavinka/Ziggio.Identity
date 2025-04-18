namespace Ziggio.Identity.Infrastructure.Configuration;

public class ServerKeyAuthorization {
  public string HeaderName { get; set; } = "";
  public string Scheme { get; set; } = null;
  public List<string> ServerKeys { get; set; } = new List<string>();
}