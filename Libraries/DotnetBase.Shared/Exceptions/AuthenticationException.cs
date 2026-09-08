using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class AuthenticationException : AppException
{
    public AuthenticationException(string message = "Authentication is required.")
        : base(message, StatusCodes.Status401Unauthorized) { }
}
