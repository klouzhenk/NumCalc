using System.Text.Json.Serialization;

namespace NumCalc.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NumCalcErrorCode
{
    [JsonStringEnumMemberName("RANGE_INVALID")]
    RangeInvalid,

    [JsonStringEnumMemberName("SYNTAX_ERROR")]
    SyntaxError,

    [JsonStringEnumMemberName("ZERO_DIVISION")]
    ZeroDivision,
    
    [JsonStringEnumMemberName("TIMEOUT")]
    Timeout,
    
    [JsonStringEnumMemberName("UNKNOWN_ERROR")]
    UnknownError
}