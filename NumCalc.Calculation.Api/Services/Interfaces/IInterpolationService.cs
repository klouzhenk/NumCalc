using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IInterpolationService
{
    InterpolationResponse SolveNewton(InterpolationRequest request);
    InterpolationResponse SolveLagrange(InterpolationRequest request);
    InterpolationResponse SolveSpline(InterpolationRequest request);
}
