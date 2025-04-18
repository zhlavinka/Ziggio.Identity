using FluentAssertions;
using System.Net;
using System.Net.Http;
using Ziggio.Identity.IntegrationTests.Models;
using static Ziggio.Identity.Domain.Constants;

namespace Ziggio.Identity.IntegrationTests.Helpers;

public static class AuthorizationCodeHelpers
{
    public static async Task<string> GetAuthorizationCodeAsync(HttpClient httpClient, string clientId, string clientSecret, string codeChallenge, string scope = "openid profile")
    {
        var redirectUri = "https://localhost:5260/signin-oidc";
        var email = "admin@zigg.io";
        var password = "ziggio";

        // get login page for antiforgery token
        var loginPageResponse = await httpClient.GetAsync("/account/login");
        loginPageResponse.EnsureSuccessStatusCode();
        var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();
        var antiforgeryToken = ExtractAntiforgeryToken(loginPageContent);

        // login user
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/account/login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "returnUrl", redirectUri },
                { "__RequestVerificationToken", antiforgeryToken }
            })
        };
        var loginResponse = await httpClient.SendAsync(loginRequest);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Found, "A successful login should result in a redirect");

        // request authorization code
        var authorizeResponse = await httpClient.GetAsync($"/connect/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope={scope}&code_challenge={codeChallenge}&code_challenge_method=S256");
        var content = await authorizeResponse.Content.ReadAsStringAsync();
        authorizeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Found, "The authorization response should result in a redirect");
        var authorizationCode = ExtractAuthorizationCode(authorizeResponse);

        return authorizationCode;
    }

    public static async Task<TokenResponse> GetAccessTokenAsync(HttpClient httpClient, string clientId, string clientSecret, string authorizationCode, string redirectUri, string codeVerifier)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            })
        };
        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
        tokenResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "The token request should succeed");
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(tokenContent);

        tokenData.Should().NotBeNull();
        tokenData!.AccessToken.Should().NotBeNullOrEmpty();
        tokenData!.ExpiresIn.Should().BeGreaterThan(0);

        return tokenData;
    }

    public static async Task<TokenResponse> RefreshTokenAsync(HttpClient httpClient, string refreshToken, string clientId, string clientSecret)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(tokenContent);

        tokenData.Should().NotBeNull();
        tokenData!.AccessToken.Should().NotBeNullOrEmpty();
        tokenData!.RefreshToken.Should().NotBeNullOrEmpty();
        tokenData!.ExpiresIn.Should().BeGreaterThan(0);

        return tokenData;
    }

    public static async Task RevokeTokenAsync(HttpClient httpClient, string refreshToken, string clientId, string clientSecret)
    {
        var revokeRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/revoke")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = refreshToken,
                ["token_type_hint"] = "refresh_token",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };
        var revokeResponse = await httpClient.SendAsync(revokeRequest);
        var content = await revokeResponse.Content.ReadAsStringAsync();
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK, "revocation should succeed even if token is already invalid");
    }

    private static string ExtractAntiforgeryToken(string htmlContent)
    {
        // Extract the antiforgery token from the HTML content (e.g., from a hidden input field)
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
        return tokenMatch.Success ? tokenMatch.Groups[1].Value : throw new InvalidOperationException("Antiforgery token not found.");
    }

    private static string ExtractAuthorizationCode(HttpResponseMessage response)
    {
        // Extract the authorization code from the response (e.g., from the query string or body)
        var location = response.Headers.Location;
        var query = System.Web.HttpUtility.ParseQueryString(location?.Query ?? string.Empty);
        return query["code"] ?? throw new InvalidOperationException("Authorization code not found.");
    }
}
