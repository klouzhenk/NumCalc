using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IOdeApiService
{
    Task<OdeResponse?> SolveEuler(OdeRequest request);
    Task<OdeResponse?> SolveEulerImproved(OdeRequest request);
    Task<OdeResponse?> SolveRungeKutta2(OdeRequest request);
    Task<OdeResponse?> SolveRungeKutta4(OdeRequest request);
    Task<OdeResponse?> SolvePicard(OdeRequest request);
    Task<OdeComparisonResponse?> GetOdeComparisonAsync(OdeComparisonRequest request);
}