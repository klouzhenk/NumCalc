using NumCalc.Shared.Common;
using NumCalc.Shared.EquationsSystems.Responses;

namespace NumCalc.Calculation.Api.Entities.EquationSystems;

public class SystemSolvingData
{
    public List<double>? Roots { get; set; }
    public List<EquationChartSeries>? ChartSeries { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
}