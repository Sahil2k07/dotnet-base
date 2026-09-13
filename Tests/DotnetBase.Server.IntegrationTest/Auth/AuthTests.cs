using System.Net;
using System.Net.Http.Json;
using DotnetBase.Contract.Auth.Request;
using DotnetBase.Contract.Auth.Response;
using DotnetBase.Contract.Common;
using DotnetBase.Server.IntegrationTest.Infrastructure;

namespace DotnetBase.Server.IntegrationTest.Auth;

public partial class AuthTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public AuthTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private async Task SignupUser(string email, string password)
    {
        var request = new SignupRequest
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signup", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<SigninResponse> SigninUser(string email, string password)
    {
        await SignupUser(email, password);

        var request = new SigninRequest { Email = email, Password = password };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/signin", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<SigninResponse>>();

        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        return result.Data;
    }
}
