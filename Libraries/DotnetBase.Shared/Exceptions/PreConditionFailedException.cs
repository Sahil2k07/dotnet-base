using Microsoft.AspNetCore.Http;

namespace DotnetBase.Shared.Exceptions;

public sealed class PreConditionFailedException : AppException
{
    public PreConditionFailedException(string message = "The request precondition failed.")
        : base(message, StatusCodes.Status412PreconditionFailed) { }
}
