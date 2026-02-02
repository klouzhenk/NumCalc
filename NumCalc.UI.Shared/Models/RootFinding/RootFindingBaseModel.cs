using System.ComponentModel.DataAnnotations;

namespace NumCalc.UI.Shared.Models.RootFinding;

// TODO : use IValidatableObject

public abstract class RootFindingBaseModel
{
    [Required(ErrorMessage = "Equation is required")]
    public string? FunctionExpression { get; set; }
    
    [Required]
    public double StartPoint { get; set; }
    
    [Required]
    public double EndPoint { get; set; }
    
    [Range(1e-10, 0.1, ErrorMessage = "Tolerance must be reasonable")]
    public double Tolerance { get; set; } = 1e-4;
}