using System.ComponentModel.DataAnnotations;

namespace NumCalc.Shared.EquationsSystems.Requests;

public class SystemSolvingRequest
{
    [Required]
    [MinLength(2)]
    public required List<string> Equations { get; set; }

    [Required]
    [MinLength(2)]
    public required List<string> Variables { get; set; }
}