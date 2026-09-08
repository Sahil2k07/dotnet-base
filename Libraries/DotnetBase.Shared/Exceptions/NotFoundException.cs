using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message = "The requested resource was not found.")
        : base(message, StatusCodes.Status404NotFound) { }
}
