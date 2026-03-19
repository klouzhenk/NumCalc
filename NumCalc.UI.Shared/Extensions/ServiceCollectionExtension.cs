using Cropper.Blazor.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace NumCalc.UI.Shared.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddNumCalcUiShared(this IServiceCollection services)
    {
        services.AddCropper();

        return services;
    }
}