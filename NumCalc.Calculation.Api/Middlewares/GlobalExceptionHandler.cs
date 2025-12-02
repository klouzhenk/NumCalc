using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NumCalc.Calculation.Api.Exceptions;

namespace NumCalc.Calculation.Api.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails() { Instance = httpContext.Request.Path };

        if (exception is CustomException calculationException)
        {
            logger.LogWarning("Validation error: {Message}. Code: {Code}", exception.Message, calculationException.ErrorCode);
            FillClientProblemDetails(problemDetails, calculationException);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            FillServerProblemDetails(problemDetails, exception);
        }
        
        httpContext.Response.StatusCode = problemDetails?.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails FillClientProblemDetails(ProblemDetails problemDetails, CustomException exception)
    {
        problemDetails.Title = "Client error";
        problemDetails.Status = StatusCodes.Status400BadRequest;
        problemDetails.Detail = exception.Message;
        problemDetails.Extensions["errorCode"] = exception.ErrorCode;

        return problemDetails;
    }
    
    private static ProblemDetails FillServerProblemDetails(ProblemDetails problemDetails, Exception exception)
    {
        problemDetails.Title = "Server Error";
        problemDetails.Status = StatusCodes.Status500InternalServerError;
        problemDetails.Detail = "An internal error occurred. Please contact support.";

        return problemDetails;
    }
}