using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Implementations.Calculation;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.HttpServices.Interfaces.Calculation;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;
using Serilog;

namespace NumCalc.UI.Shared.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddNumCalcUiSharedServices(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddScoped<IUiStateService, UiStateService>();
        services.AddScoped<IPdfExportService, PdfExportService>();
        PdfExportService.ConfigureQuestPdfLicense();
        services.AddCropper();

        return services;
    }

    public static IServiceCollection AddSharedLogging(this IServiceCollection services, string logFilePath)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(
                path:logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }

    public static IServiceCollection AddCalculationApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUri = new Uri(configuration["Apis:CalculationApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: Apis:CalculationApi:BaseUrl"));

        services.AddApiClient<IRootFindingApiService, RootFindingApiService>(baseUri);
        services.AddApiClient<IEquationSystemApiService, EquationSystemApiService>(baseUri);
        services.AddApiClient<IIntegrationApiService, IntegrationApiService>(baseUri);
        services.AddApiClient<IInterpolationApiService, InterpolationApiService>(baseUri);
        services.AddApiClient<IDifferentiationApiService, DifferentiationApiService>(baseUri);
        services.AddApiClient<IOptimizationApiService, OptimizationApiService>(baseUri);
        services.AddApiClient<IOdeApiService, OdeApiService>(baseUri);
        services.AddApiClient<IOcrApiService, OcrApiService>(baseUri);

        return services;
    }

    public static IServiceCollection AddUserApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationCore();
        services.AddScoped<IAuthStateService, AuthStateService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        var baseUri = new Uri(configuration["Apis:UserApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: Apis:UserApi:BaseUrl"));

        services.AddApiClient<IAuthApiService, AuthApiService>(baseUri);
        services.AddApiClient<IUserApiService, UserApiService>(baseUri);
        services.AddApiClient<ICalculationHistoryApiService, CalculationHistoryApiService>(baseUri);
        services.AddApiClient<ISavedInputApiService, SavedInputApiService>(baseUri);
        services.AddApiClient<ISavedFileApiService, SavedFileApiService>(baseUri);

        return services;
    }

    private static void AddApiClient<TInterface, TImplementation>(this IServiceCollection services, Uri baseUri)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddHttpClient<TInterface, TImplementation>(client =>
        {
            client.BaseAddress = baseUri;
        });
    }
}