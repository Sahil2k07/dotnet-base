using System.Net;
using System.Net.Http.Json;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Contract.Common;

namespace DotnetBase.Server.IntegrationTest.Auth;

public partial class AuthTests
{
    [Fact]
    public async Task Signup_WithValidRequest_ReturnsOk()
    {
        var request = new SignupRequest
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Password = "Password@123",
            FirstName = "Test",
            LastName = "User",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SigninResponse>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User signed up successful", result.Message);

        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));
    }

    [Fact]
    public async Task Signup_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"test-{Guid.NewGuid()}@example.com";

        var request = new SignupRequest
        {
            Email = email,
            Password = "Password@123",
            FirstName = "Test",
            LastName = "User",
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Signup_WithInvalidEmail_ReturnsUnprocessableEntity()
    {
        var request = new SignupRequest
        {
            Email = "invalid-email",
            Password = "Password@123",
            FirstName = "Test",
            LastName = "User",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WithMissingRequiredFields_ReturnsUnprocessableEntity()
    {
        var request = new
        {
            Email = "",
            Password = "",
            FirstName = "",
            LastName = "",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WithFirstNameTooLong_ReturnsUnprocessableEntity()
    {
        var request = new SignupRequest
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Password = "Password@123",
            FirstName = new string('A', 51),
            LastName = "User",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WithLastNameTooLong_ReturnsUnprocessableEntity()
    {
        var request = new SignupRequest
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Password = "Password@123",
            FirstName = "Test",
            LastName = new string('A', 51),
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
