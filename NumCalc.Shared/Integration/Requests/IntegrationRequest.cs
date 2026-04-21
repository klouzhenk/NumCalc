using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.Integration;

namespace NumCalc.Shared.Integration.Requests;

public class IntegrationRequest
{
    [Required]
    public IntegrationInputMode Mode { get; set; }

    public string? FunctionExpression { get; set; }

    [Required]
    public double LowerBound { get; set; }

    [Required]
    public double UpperBound { get; set; }

    [Range(1, 10000)]
    public int Intervals { get; set; } = 100;

    public RectangleVariant? RectangleVariant { get; set; }
}
