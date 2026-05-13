using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IOptimizationApiService
{
    Task<OptimizationResponse?> OptimizeUniformSearchAsync(OptimizationRequest request);
    Task<OptimizationResponse?> OptimizeGoldenSectionAsync(OptimizationRequest request);
    Task<OptimizationResponse?> OptimizeGradientDescentAsync(GradientDescentRequest request);
    Task<OptimizationComparisonResponse?> GetOptimizationComparisonAsync(OptimizationComparisonRequest request);
}