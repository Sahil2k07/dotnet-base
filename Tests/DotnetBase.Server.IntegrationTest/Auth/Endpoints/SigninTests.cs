using System.Net;
using System.Net.Http.Json;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Contract.Common;

namespace DotnetBase.Server.IntegrationTest.Auth;

public partial class AuthTests
{
    [Fact]
    public async Task Signin_WithValidCredentials_ReturnsOk()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password@123";

        await SignupUser(email, password);

        var request = new SigninRequest { Email = email, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SigninResponse>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User signed in successfully", result.Message);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));
    }

    [Fact]
    public async Task Signin_WithInvalidPassword_ReturnsBadRequest()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";

        await SignupUser(email, "Password@123");

        var request = new SigninRequest { Email = email, Password = "WrongPassword@123" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signin_WithNonExistentEmail_ReturnsNotFound()
    {
        var request = new SigninRequest
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "Password@123",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Signin_WithInvalidEmail_ReturnsUnprocessableEntity()
    {
        var request = new SigninRequest { Email = "invalid-email", Password = "Password@123" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signin_WithMissingRequiredFields_ReturnsUnprocessableEntity()
    {
        var request = new { Email = "", Password = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signin_WithEmptyPassword_ReturnsUnprocessableEntity()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";

        await SignupUser(email, "Password@123");

        var request = new SigninRequest { Email = email, Password = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
