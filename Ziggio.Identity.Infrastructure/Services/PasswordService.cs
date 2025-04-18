using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using Ziggio.Identity.Domain.Services;

namespace Ziggio.Identity.Infrastructure.Services;
public class PasswordService : IPasswordService {
  const string Lowers = "abcdefghijklmnopqursuvwxyz";
  const string Uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  const string Numbers = "0123456789";
  const string Specials = @"!@£$%^&*()#€";

  public string GeneratePassword(int length, bool userLowercase = true, bool useUppercase = true, bool useNumbers = true, bool useSpecials = true) {
    char[] password = new char[length];
    string charSet = "";
    var random = new Random();

    if (userLowercase)
      charSet += Lowers;

    if (useUppercase)
      charSet += Uppers;

    if (useNumbers)
      charSet += Numbers;

    if (useSpecials)
      charSet += Specials;

    for (int i = 0; i < length; i++) {
      password[i] = charSet[random.Next(charSet.Length - 1)];
    }

    return new string(password);
  }

  public byte[] GenerateSalt() {
    var randomNumber = new byte[32];

    using (var rng = RandomNumberGenerator.Create()) {
      rng.GetBytes(randomNumber);
    }

    return randomNumber;
  }

  public byte[] HashPassword(string password, byte[] salt) {
    var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
    argon2.Salt = salt;
    argon2.DegreeOfParallelism = 8;
    argon2.Iterations = 4;
    argon2.MemorySize = 1024 * 1024; // 1GB

    return argon2.GetBytes(16);
  }
}