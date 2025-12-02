using NumCalc.Shared.Enums;

namespace NumCalc.Calculation.Api.Exceptions;

public class CustomException(NumCalcErrorCode errorCode, string message) : Exception(message)
{
    public NumCalcErrorCode ErrorCode { get; set; } = errorCode;
}