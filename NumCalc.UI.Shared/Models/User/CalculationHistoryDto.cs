using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Models.User;

public class CalculationHistoryDto
{
    public Guid Id { get; set; }
    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string InputsJson { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
}
