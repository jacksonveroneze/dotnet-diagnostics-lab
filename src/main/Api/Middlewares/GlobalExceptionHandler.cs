using Microsoft.AspNetCore.Diagnostics;

namespace JacksonVeroneze.NET.GRPCServer.Api.Middlewares;

internal sealed class CustomExceptionHandler(
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        const int defaultStatus = StatusCodes.Status500InternalServerError;

        ProblemDetailsContext problemDetailsCtx = new()
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails =
            {
                Title = "An error occurred",
                Detail = exception.Message,
                Type = exception.GetType().Name,
                Status = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    _ => defaultStatus,
                },
            },
        };

        return problemDetailsService
            .TryWriteAsync(problemDetailsCtx);
    }
}
