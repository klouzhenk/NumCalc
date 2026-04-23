using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IOptimizationService
{
    OptimizationResponse SolveUniformSearch(OptimizationRequest request);
    OptimizationResponse SolveGoldenSection(OptimizationRequest request);
    OptimizationResponse SolveGradientDescent(GradientDescentRequest request);
    OptimizationComparisonResponse Compare(OptimizationComparisonRequest request);
}
