using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.Exceptions;

public class CustomException(UserErrorCode errorCode, string message, int statusCode) : Exception(message)
{
    public UserErrorCode ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}
