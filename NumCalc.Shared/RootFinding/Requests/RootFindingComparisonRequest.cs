using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.RootFinding;

namespace NumCalc.Shared.RootFinding.Requests;

/// <summary>
/// Data transfer object for root-finding comparison calculation requests.
/// </summary>
public class RootFindingComparisonRequest
{
    /// <summary>
    /// The methods to compare.
    /// </summary>
    /// TODO : generate examples
    public IEnumerable<RootFindingMethod>? Methods { get; set; }
    
    /// <summary>
    /// The mathematical function expression using Python or LaTeX syntax.
    /// </summary>
    /// <example>x**3 - x - 2</example>
    [Required]
    [DefaultValue("x**3 - x - 2")]
    public string FunctionExpression { get; set; } = string.Empty;

    /// <summary>
    /// The start of the search interval (often referred to as 'a').
    /// </summary>
    /// <example>1.0</example>
    [Required]
    [DefaultValue(1.0)]
    public double StartRange { get; set; }

    /// <summary>
    /// The end of the search interval (often referred to as 'b').
    /// </summary>
    /// <example>2.0</example>
    [Required]
    [DefaultValue(2.0)]
    public double EndRange { get; set; }

    /// <summary>
    /// The desired accuracy (tolerance) for the solution.
    /// The calculation stops when the difference between iterations is less than this value.
    /// </summary>
    /// <example>0.001</example>
    [Range(1e-10, 0.1, ErrorMessage = "Tolerance must be between 1e-15 and 0.1")]
    [DefaultValue(0.001)]
    public double Tolerance { get; set; } = 0.001;
}