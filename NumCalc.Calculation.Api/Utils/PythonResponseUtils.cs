using System.Text.Json;
using NumCalc.Calculation.Api.Entities;
using NumCalc.Calculation.Api.Exceptions;
using NumCalc.Shared.Enums;

namespace NumCalc.Calculation.Api.Utils;

public static class PythonResponseUtils
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
    
    public static T UnwrapOrThrow<T>(this string jsonEnvelope)
    {
        PythonResponse<T>? envelope;
        
        try 
        {
            envelope = JsonSerializer.Deserialize<PythonResponse<T>>(jsonEnvelope, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new CustomException(NumCalcErrorCode.InvalidJson, $"Failed to parse Python response from gotten JSON: {ex.Message}");
        }

        if (envelope is null)
            throw new CustomException(NumCalcErrorCode.EmptyResponse, "Python response is null");

        if (envelope.Failure is not null)
        {
            var errorCode = Enum.TryParse<NumCalcErrorCode>(envelope.Failure.Code, true, out var code) 
                ? code 
                : NumCalcErrorCode.UnknownError;

            throw new CustomException(errorCode, envelope.Failure.Message);
        }

        if (envelope.Success is null)
            throw new CustomException(NumCalcErrorCode.UnknownError, "Response was not generic failure, but data is missing.");

        return envelope.Success;
    }
}