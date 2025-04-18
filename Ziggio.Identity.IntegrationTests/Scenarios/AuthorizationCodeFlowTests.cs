using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ziggio.Identity.Infrastructure.Data.Contexts;
using Ziggio.Identity.IntegrationTests.Helpers;

namespace Ziggio.Identity.IntegrationTests.Scenarios;

[TestClass]
public class AuthorizationCodeFlowTests : TestBase
{
    private static AuthorizationCodeFlowTests _scenario = default!;
    private static IConfiguration _configuration = default!;
    private static HttpClient _httpClient = default!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _scenario = new AuthorizationCodeFlowTests();
        _scenario.InitializeScenario();

        _configuration = _scenario.Configuration;
        _httpClient = _scenario.HttpClient;
    }

    [TestMethod]
    public async Task AuthorizationCode_ShouldReturnAccessToken()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        // act 1 - get authorization code
        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(_httpClient, clientId, clientSecret, codeChallenge);

        // act 2 - exchange authorization code for access token
        var tokenData = await AuthorizationCodeHelpers.GetAccessTokenAsync(_httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);

        // assert
        tokenData.Should().NotBeNull();
        tokenData!.AccessToken.Should().NotBeNullOrEmpty();
        tokenData.ExpiresIn.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task AuthorizationCode_CannotBeReused()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        // act 1 - get authorization code
        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(
            _httpClient,
            clientId,
            clientSecret,
            codeChallenge,
            "openid profile");

        // act 2 - exchange code for access token
        var firstResponse = await AuthorizationCodeHelpers.GetAccessTokenAsync(
            _httpClient,
            clientId,
            clientSecret,
            authorizationCode,
            redirectUri,
            codeVerifier);

        firstResponse.AccessToken.Should().NotBeNullOrEmpty();

        // act 3 - try to reuse the same code again (should fail)
        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = authorizationCode,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code_verifier"] = codeVerifier
            })
        };

        var secondResponse = await _httpClient.SendAsync(secondRequest);
        var secondContent = await secondResponse.Content.ReadAsStringAsync();

        // assert that a 400 status code is returned due to invalid reuse of the authorization code
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // assert that the error returned is 'invalid_grant'
        var secondJson = JsonDocument.Parse(secondContent);
        secondJson.RootElement.GetProperty("error").GetString().Should().Be("invalid_grant");
    }

    [TestMethod]
    public async Task RefreshToken_ExchangeForNewAccessToken()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        // act 1 - get authorization code
        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(_httpClient, clientId, clientSecret, codeChallenge, "openid profile offline_access");

        // act 2 - exchange authorization code for access token
        var tokenData = await AuthorizationCodeHelpers.GetAccessTokenAsync(_httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);
        tokenData!.RefreshToken.Should().NotBeNullOrEmpty();

        // act 3 - refresh access token
        var newTokenData = await AuthorizationCodeHelpers.RefreshTokenAsync(_httpClient, tokenData.RefreshToken, clientId, clientSecret);

        // assert
        newTokenData.RefreshToken.Should().NotBeNullOrEmpty();
        newTokenData.AccessToken.Should().NotBe(tokenData.AccessToken, "A new access token should have been issued");
        newTokenData.RefreshToken.Should().NotBe(tokenData.RefreshToken, "A new refresh token should have been issued");
    }

    [TestMethod]
    public async Task RefreshToken_Rotate_ShouldInvalidateOldToken()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        // get authorization code
        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(
            _httpClient, clientId, clientSecret, codeChallenge, "openid profile offline_access");

        // exchange code for initial tokens
        var initialToken = await AuthorizationCodeHelpers.GetAccessTokenAsync(
            _httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);

        initialToken.RefreshToken.Should().NotBeNullOrEmpty();

        // use refresh token to get new tokens (rotation)
        var refreshRequest1 = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", initialToken.RefreshToken },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            })
        };

        var response1 = await _httpClient.SendAsync(refreshRequest1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var content1 = await response1.Content.ReadAsStringAsync();
        var tokenJson1 = JsonDocument.Parse(content1);

        var newRefreshToken = tokenJson1.RootElement.GetProperty("refresh_token").GetString();
        newRefreshToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBe(initialToken.RefreshToken, "refresh token should rotate");

        // try using the old refresh token again (should fail)
        var reuseRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", initialToken.RefreshToken },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            })
        };

        var reuseResponse = await _httpClient.SendAsync(reuseRequest);
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var reuseContent = await reuseResponse.Content.ReadAsStringAsync();
        var reuseJson = JsonDocument.Parse(reuseContent);

        reuseJson.RootElement.GetProperty("error").GetString().Should().Be("invalid_grant");
    }

    [TestMethod]
    public async Task RefreshToken_Expired_CannotBeUsed()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(
            _httpClient, clientId, clientSecret, codeChallenge, "openid profile offline_access");

        var tokenData = await AuthorizationCodeHelpers.GetAccessTokenAsync(
            _httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);

        tokenData.RefreshToken.Should().NotBeNullOrEmpty();

        // act 1 - expire the token in the DB
        using (var scope = _scenario.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

            var refreshTokenHash = HashRefreshToken(tokenData.RefreshToken);

            var token = await dbContext.Set<OpenIddictEntityFrameworkCoreToken>()
                .Where(t => t.ReferenceId == refreshTokenHash)
                .FirstOrDefaultAsync();

            token.Should().NotBeNull("because the refresh token should exist in the database");

            token.ExpirationDate = DateTime.UtcNow.AddMinutes(-1); // Expire it
            await dbContext.SaveChangesAsync();
        }

        // try to use the now-expired refresh token
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = tokenData.RefreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        var refreshResponse = await _httpClient.SendAsync(refreshRequest);
        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();

        // assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        refreshContent.Should().Contain("invalid_grant");
    }

    [TestMethod]
    public async Task RefreshToken_ReuseShouldFail()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(
            _httpClient, clientId, clientSecret, codeChallenge, "openid profile offline_access");

        var tokenData = await AuthorizationCodeHelpers.GetAccessTokenAsync(
            _httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);

        var refreshToken = tokenData.RefreshToken;
        refreshToken.Should().NotBeNullOrEmpty();

        // act 1 - first use of refresh token
        var firstRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        var firstResponse = await _httpClient.SendAsync(firstRefreshRequest);
        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstContent.Should().Contain("access_token");

        // act 2 - attempt reuse of the same token
        var secondRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        var secondResponse = await _httpClient.SendAsync(secondRefreshRequest);
        var secondContent = await secondResponse.Content.ReadAsStringAsync();

        // assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        secondContent.Should().Contain("invalid_grant");
    }

    [TestMethod]
    public async Task RefreshToken_Revoke_CannotBeUsed()
    {
        // arrange
        var clientId = _configuration["Ziggio:Testing:AuthorizationCode:ClientId"];
        var clientSecret = _configuration["Ziggio:Testing:AuthorizationCode:ClientSecret"];
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";
        var (codeVerifier, codeChallenge) = PkceCodeVerifier.Generate();

        var authorizationCode = await AuthorizationCodeHelpers.GetAuthorizationCodeAsync(
            _httpClient, clientId, clientSecret, codeChallenge, "openid profile offline_access");

        var tokenData = await AuthorizationCodeHelpers.GetAccessTokenAsync(
            _httpClient, clientId, clientSecret, authorizationCode, redirectUri, codeVerifier);

        tokenData.RefreshToken.Should().NotBeNullOrEmpty();

        // act 1 - revoke refresh token
        await AuthorizationCodeHelpers.RevokeTokenAsync(_httpClient, tokenData.RefreshToken, clientId, clientSecret);

        // act 2 - try to use revoked token
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = tokenData.RefreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        var refreshResponse = await _httpClient.SendAsync(refreshRequest);
        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();

        // assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        refreshContent.Should().Contain("invalid_grant");
    }

    /// <summary>
    /// A helper function to hash the refresh token, simulating how it might be stored in the database.
    /// </summary>
    private string HashRefreshToken(string refreshToken)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
