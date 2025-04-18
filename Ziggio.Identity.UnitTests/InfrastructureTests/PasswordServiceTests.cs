using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ziggio.Identity.Infrastructure.Services;

namespace Ziggio.Identity.UnitTests.InfrastructureTests;

[TestClass]
public class PasswordServiceTests
{
    private PasswordService _passwordService;

    [TestInitialize]
    public void Init()
    {
        _passwordService = new PasswordService();
    }

    [TestMethod]
    public void SaltLengthIsGreaterThanZero()
    {
        var salt = Convert.ToBase64String(_passwordService.GenerateSalt());
        salt.Length.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public void RandomPasswordLengthEqualsInput()
    {
        var password = _passwordService.GeneratePassword(18);
        password.Length.Should().Be(18);
    }

    [TestMethod]
    public void PasswordHashComparisonMatches()
    {
        // use known password
        var password = "Th1s!sAn3xpected_P@ssword!";
        // generate random salt
        var salt = _passwordService.GenerateSalt();
        // convert to string like will be stored in db
        var saltString = Convert.ToBase64String(salt);
        // hash password
        var hashedPassword = Convert.ToBase64String(_passwordService.HashPassword(password, salt));
        // convert string salt back to byte[] like retrieved from db
        var saltBuffer = Convert.FromBase64String(saltString);
        // hash password with "stored" salt
        var rehashedPassword = Convert.ToBase64String(_passwordService.HashPassword(password, saltBuffer));
        // compare
        hashedPassword.Should().Be(rehashedPassword);
    }
}