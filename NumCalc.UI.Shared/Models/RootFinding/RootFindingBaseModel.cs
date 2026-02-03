using System.ComponentModel.DataAnnotations;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.RootFinding;

// TODO : use IValidatableObject

public abstract class RootFindingBaseModel
{
    [Required(ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "EquationIsRequired")]
    public string? FunctionExpression { get; set; }
    
    [Required]
    public double StartPoint { get; set; }
    
    [Required]
    public double EndPoint { get; set; }
    
    [Range(1e-10, 0.1, ErrorMessageResourceType = typeof(Localization), ErrorMessageResourceName = "ToleranceMustBeReasonable")]
    public double Tolerance { get; set; } = 1e-4;
}