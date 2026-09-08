using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(
        string message = "You do not have permission to access this resource."
    )
        : base(message, StatusCodes.Status403Forbidden) { }
}
