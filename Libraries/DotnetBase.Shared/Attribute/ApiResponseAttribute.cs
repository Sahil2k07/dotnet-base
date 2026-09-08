using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetBase.Shared.Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ApiResponseAttribute : ActionFilterAttribute
{
    private readonly string _message;

    public ApiResponseAttribute(string message = "Request completed successfully.")
    {
        _message = message;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult result)
        {
            return;
        }

        // Only wrap successful responses.
        if (result.StatusCode is not null && (result.StatusCode < 200 || result.StatusCode >= 300))
        {
            return;
        }

        // Don't wrap an ApiResponse that has already been created.
        if (HasApiResponseStructure(result.Value))
        {
            return;
        }

        result.Value = new
        {
            Success = true,
            Message = _message,
            Data = result.Value,
            Errors = (object?)null,
        };
    }

    private static bool HasApiResponseStructure(object? value)
    {
        if (value is null)
        {
            return false;
        }

        var properties = value.GetType().GetProperties();

        return properties.Any(p =>
                string.Equals(p.Name, "Success", StringComparison.OrdinalIgnoreCase)
            )
            && properties.Any(p =>
                string.Equals(p.Name, "Data", StringComparison.OrdinalIgnoreCase)
            );
    }
}
