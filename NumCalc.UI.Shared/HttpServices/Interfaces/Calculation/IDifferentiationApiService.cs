using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IDifferentiationApiService
{
    Task<DifferentiationResponse?> DifferentiateFiniteDiffAsync(DifferentiationRequest request, FiniteDiffVariant variant);
    Task<DifferentiationResponse?> DifferentiateLagrangeAsync(DifferentiationRequest request);
    Task<DifferentiationComparisonResponse?> GetDifferentiationComparisonAsync(DifferentiationComparisonRequest request);
}