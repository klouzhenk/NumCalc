using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IInterpolationApiService
{
    Task<InterpolationResponse?> InterpolateNewtonAsync(InterpolationRequest request);
    Task<InterpolationResponse?> InterpolateLagrangeAsync(InterpolationRequest request);
    Task<InterpolationResponse?> InterpolateSplineAsync(InterpolationRequest request);
    Task<InterpolationComparisonResponse?> GetInterpolationComparisonAsync(InterpolationComparisonRequest request);
}