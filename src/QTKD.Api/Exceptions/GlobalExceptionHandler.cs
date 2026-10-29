using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace QTKD.Api.Exceptions;

/// <summary>
/// Xử lý exception tập trung cho toàn bộ pipeline theo chuẩn RFC 7807 ProblemDetails.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is AppException appException)
        {
            _logger.LogWarning("Handled {ExceptionType}: {Message}", exception.GetType().Name, exception.Message);

            problem = appException is ValidationException validation
                ? new ValidationProblemDetails(validation.Errors)
                : new ProblemDetails();

            problem.Status = appException.StatusCode;
            problem.Title = appException.Title;
            problem.Detail = appException.Message;
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception");

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Please try again later or contact the administrator."
            };
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}

