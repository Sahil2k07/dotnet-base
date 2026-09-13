using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DotnetBase.Contract.Common;
using DotnetBase.Contract.User.Response;

namespace DotnetBase.Server.IntegrationTest.User;

public partial class UserTests
{
    [Fact]
    public async Task Me_WithValidAccessToken_ReturnsUserInformation()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var response = await _client.GetAsync("/api/v1/User/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<UserInformationResponse>
        >();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.UserId > 0);
        Assert.True(result.Data.UserProfileId > 0);
        Assert.NotNull(result.Data.Roles);
        Assert.NotNull(result.Data.Permissions);
    }

    [Fact]
    public async Task Me_WithoutAccessToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/v1/User/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithInvalidAccessToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "invalid-access-token"
        );

        var response = await _client.GetAsync("/api/v1/User/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidAccessToken_ReturnsUserRole()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var response = await _client.GetAsync("/api/v1/User/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<UserInformationResponse>
        >();

        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        Assert.Contains("USER", result.Data.Roles);
    }

    [Fact]
    public async Task Me_WithValidAccessToken_ReturnsExpectedPermissions()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        var signinResponse = await SigninUser(email, password);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            signinResponse.AccessToken
        );

        var response = await _client.GetAsync("/api/v1/User/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<UserInformationResponse>
        >();

        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        Assert.NotNull(result.Data.Permissions);
    }
}
