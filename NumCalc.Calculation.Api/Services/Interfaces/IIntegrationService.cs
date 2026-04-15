using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;

namespace NumCalc.Calculation.Api.Services.Interfaces;

public interface IIntegrationService
{
    IntegrationResponse SolveRectangle(IntegrationRequest request);
    IntegrationResponse SolveTrapezoid(IntegrationRequest request);
    IntegrationResponse SolveSimpson(IntegrationRequest request);
}
