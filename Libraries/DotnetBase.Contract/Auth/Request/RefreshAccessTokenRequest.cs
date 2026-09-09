namespace DotnetBase.Contract.Auth.Request;

public sealed class RefreshAccessTokenRequest
{
    public required string RefreshToken { get; set; }
}
