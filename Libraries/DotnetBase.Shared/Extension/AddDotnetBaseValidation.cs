using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Shared.Extension;

public static class ValidationExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetBaseValidation()
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    if (context.ProblemDetails is HttpValidationProblemDetails validation)
                    {
                        context.ProblemDetails = new ApiResponseProblemDetails(validation.Errors);

                        context.HttpContext.Response.StatusCode =
                            StatusCodes.Status422UnprocessableEntity;
                    }
                };
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context
                        .ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return new ObjectResult(
                        new
                        {
                            Success = false,
                            Message = "One or more validation errors occurred.",
                            Data = (object?)null,
                            Errors = errors,
                        }
                    )
                    {
                        StatusCode = StatusCodes.Status422UnprocessableEntity,
                    };
                };
            });

            return services;
        }
    }

    private sealed class ApiResponseProblemDetails : ProblemDetails
    {
        public ApiResponseProblemDetails(IDictionary<string, string[]> errors)
        {
            Extensions["success"] = false;
            Extensions["message"] = "One or more validation errors occurred.";
            Extensions["data"] = null;
            Extensions["errors"] = errors;
        }
    }
}
