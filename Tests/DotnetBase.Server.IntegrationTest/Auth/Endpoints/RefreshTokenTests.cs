using System.Net;
using System.Net.Http.Json;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Contract.Common;

namespace DotnetBase.Server.IntegrationTest.Auth;

public partial class AuthTests
{
    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsOk()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        var request = new RefreshAccessTokenRequest { RefreshToken = signinResponse.RefreshToken };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SigninResponse>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_RotatesTokens()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        var request = new RefreshAccessTokenRequest { RefreshToken = signinResponse.RefreshToken };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SigninResponse>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));

        Assert.NotEqual(signinResponse.AccessToken, result.Data.AccessToken);

        Assert.NotEqual(signinResponse.RefreshToken, result.Data.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        var request = new RefreshAccessTokenRequest { RefreshToken = "invalid-refresh-token" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithEmptyRefreshToken_ReturnsUnprocessableEntity()
    {
        var request = new RefreshAccessTokenRequest { RefreshToken = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
