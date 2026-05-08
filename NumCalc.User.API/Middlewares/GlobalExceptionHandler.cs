using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NumCalc.User.Application.Exceptions;

namespace NumCalc.User.API.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails { Instance = httpContext.Request.Path };

        if (exception is CustomException customException)
        {
            logger.LogWarning("Client error: {Message}. Code: {Code}", exception.Message, customException.ErrorCode);
            FillClientProblemDetails(problemDetails, customException);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            FillServerProblemDetails(problemDetails);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static void FillClientProblemDetails(ProblemDetails problemDetails, CustomException exception)
    {
        problemDetails.Title = "Client error";
        problemDetails.Status = exception.StatusCode;
        problemDetails.Detail = exception.Message;
        problemDetails.Extensions["errorCode"] = exception.ErrorCode;
    }
    
    private static void FillServerProblemDetails(ProblemDetails problemDetails)
    {
        problemDetails.Title = "Server Error";
        problemDetails.Status = StatusCodes.Status500InternalServerError;
        problemDetails.Detail = "An internal error occurred. Please contact support.";
    }
}