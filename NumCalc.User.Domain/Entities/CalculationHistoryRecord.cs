using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Domain.Entities;

public class CalculationHistoryRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public CalculationType Type { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string InputsJson { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
}
