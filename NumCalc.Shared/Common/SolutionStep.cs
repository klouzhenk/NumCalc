namespace NumCalc.Shared.Common;
using System.Text.Json.Serialization;

public class SolutionStep
{
    [JsonPropertyName("step_index")]
    public int StepIndex { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("latex_formula")]
    public string? LatexFormula { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}