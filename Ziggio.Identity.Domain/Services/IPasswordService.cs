namespace Ziggio.Identity.Domain.Services;

public interface IPasswordService {
  string GeneratePassword(int length, bool userLowercase, bool useUppercase, bool useNumbers, bool useSpecials);
  byte[] GenerateSalt();
  byte[] HashPassword(string password, byte[] salt);
}