using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message = "The request is invalid.")
        : base(message, StatusCodes.Status400BadRequest) { }
}
