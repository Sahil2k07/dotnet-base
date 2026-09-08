using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class UnprocessableEntityException : AppException
{
    public UnprocessableEntityException(string message = "The request could not be processed.")
        : base(message, StatusCodes.Status422UnprocessableEntity) { }
}
