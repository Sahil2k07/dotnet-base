using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(
        string message = "The request conflicts with the current state of the resource."
    )
        : base(message, StatusCodes.Status409Conflict) { }
}
