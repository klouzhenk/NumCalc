using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.Interpolation;

namespace NumCalc.Shared.Interpolation.Requests;

public class InterpolationRequest
{
    [Required]
    public InterpolationInputMode Mode { get; set; }

    public string? FunctionExpression { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "At least 2 x-nodes are required")]
    public required List<double> XNodes { get; set; }

    public List<double>? YValues { get; set; }

    [Required]
    public double QueryPoint { get; set; }
}
