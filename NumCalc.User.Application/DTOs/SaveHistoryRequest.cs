using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.DTOs;

public class SaveHistoryRequest
{
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string InputsJson { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
}
