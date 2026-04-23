using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.Integration;

namespace NumCalc.Shared.Integration.Requests;

public class IntegrationComparisonRequest
{
    public IEnumerable<IntegrationComparisonMethod>? Methods { get; set; }

    [Required]
    public string FunctionExpression { get; set; } = string.Empty;

    [Required]
    public double LowerBound { get; set; }

    [Required]
    public double UpperBound { get; set; }

    [Range(1, 10000)]
    public int Intervals { get; set; } = 100;
}
