using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;

namespace NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;

public interface IIntegrationApiService
{
    Task<IntegrationResponse?> IntegrateRectangleAsync(IntegrationRequest request);
    Task<IntegrationResponse?> IntegrateTrapezoidAsync(IntegrationRequest request);
    Task<IntegrationResponse?> IntegrateSimpsonAsync(IntegrationRequest request);
    Task<IntegrationComparisonResponse?> GetIntegrationComparisonAsync(IntegrationComparisonRequest request);
}