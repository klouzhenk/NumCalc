using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Domain.Entities;

public class SavedInput : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public CalculationDomain Domain { get; set; }
    public string InputsJson { get; set; } = string.Empty;
}
