namespace DotnetBase.Contract.Auth.Response;

public sealed class SigninResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}
