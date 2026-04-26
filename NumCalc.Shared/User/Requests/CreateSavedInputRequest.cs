using NumCalc.Shared.User.Enums;

namespace NumCalc.Shared.User.Requests;

public class CreateSavedInputRequest
{
    public string Name { get; set; } = string.Empty;
    public CalculationType Type { get; set; }
    public string InputsJson { get; set; } = string.Empty;
}
