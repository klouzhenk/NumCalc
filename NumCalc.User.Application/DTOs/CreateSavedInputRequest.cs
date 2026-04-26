using NumCalc.User.Domain.Enums;

namespace NumCalc.User.Application.DTOs;

public class CreateSavedInputRequest
{
    public string Name { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string InputsJson { get; set; } = string.Empty;
}
