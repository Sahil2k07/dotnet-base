using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DotnetBase.Contract.Auth.Request;

namespace DotnetBase.Server.IntegrationTest.Auth;

public partial class AuthTests
{
    [Fact]
    public async Task Signout_WithValidRefreshToken_ReturnsOk()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var request = new RefreshAccessTokenRequest { RefreshToken = signinResponse.RefreshToken };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signout", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Signout_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var request = new RefreshAccessTokenRequest { RefreshToken = "invalid-refresh-token" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signout", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Signout_WithEmptyRefreshToken_ReturnsUnauthorized()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var request = new RefreshAccessTokenRequest { RefreshToken = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signout", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signout_WithValidRefreshToken_RevokesSession()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var signoutRequest = new RefreshAccessTokenRequest
        {
            RefreshToken = signinResponse.RefreshToken,
        };

        var signoutResponse = await _client.PostAsJsonAsync("/api/v1/Auth/signout", signoutRequest);

        Assert.Equal(HttpStatusCode.OK, signoutResponse.StatusCode);

        var refreshRequest = new RefreshAccessTokenRequest
        {
            RefreshToken = signinResponse.RefreshToken,
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", refreshRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }
}
