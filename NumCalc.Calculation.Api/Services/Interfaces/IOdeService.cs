using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IOdeService
{
    OdeResponse SolveEuler(OdeRequest request);
    OdeResponse SolveEulerImproved(OdeRequest request);
    OdeResponse SolveRungeKutta2(OdeRequest request);
    OdeResponse SolveRungeKutta4(OdeRequest request);
    OdeResponse SolvePicard(OdeRequest request);
    OdeComparisonResponse Compare(OdeComparisonRequest request);
}
