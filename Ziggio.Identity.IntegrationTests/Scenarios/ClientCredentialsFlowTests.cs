using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Ziggio.Identity.Domain;

namespace Ziggio.Identity.IntegrationTests.Scenarios;

[TestClass]
public class ClientCredentialsFlowTests : TestBase
{
    private static ClientCredentialsFlowTests _scenario = default!;
    private static IConfiguration _configuration = default!;
    private static HttpClient _httpClient = default!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _scenario = new ClientCredentialsFlowTests();
        _scenario.InitializeScenario();

        _configuration = _scenario.Configuration;
        _httpClient = _scenario.HttpClient;
    }

    [TestMethod]
    public async Task ClientCredentials_RequestsAccessToken_ReturnsAccessToken()
    {
        var clientId = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientId"];
        var clientSecret = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientSecret"];

        // arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            ])
        };

        // act
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "the request is valid");

        var json = JsonDocument.Parse(content);
        json.RootElement.TryGetProperty("access_token", out var token).Should().BeTrue("an access token should be returned");
        token.GetString().Should().NotBeNullOrWhiteSpace("the access token should be present and not empty");
    }

    [TestMethod]
    public async Task ClientCredentials_RequestsInvalidScope_ReturnsError()
    {
        var clientId = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientId"];
        var clientSecret = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientSecret"];

        // arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", "some_invalid_scope")
            ])
        };

        // act
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "the scope is not valid");
    }

    [TestMethod]
    public async Task ClientCredentials_RequestsValidScope_ReturnsAccessToken()
    {
        var clientId = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientId"];
        var clientSecret = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientSecret"];

        // arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", Constants.Resources.Ziggio.Testing.ClientCredentialsTest)
            ])
        };

        // act
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "the request is valid");

        var json = JsonDocument.Parse(content);
        json.RootElement.TryGetProperty("access_token", out var token).Should().BeTrue("an access token should be returned");
        token.GetString().Should().NotBeNullOrWhiteSpace("the access token should be present and not empty");
    }

    [TestMethod]
    public async Task ClientCredentials_AccessToken_Expires()
    {
        var clientId = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientId"];
        var clientSecret = _scenario.Configuration["Ziggio:Testing:ClientCredentials:ClientSecret"];

        // arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            ])
        };

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);
        var token = json.RootElement.GetProperty("access_token").GetString();

        // wait for token to expire
        await Task.Delay(6000);

        // act - make an authenticated request with the expired token
        var apiRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
        apiRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var expiredResponse = await _httpClient.SendAsync(apiRequest);

        // assert
        expiredResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
