using Cropper.Blazor.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NumCalc.UI.Shared.HttpServices;
using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Interfaces;
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
        const string baseApiUrl = "http://localhost:5229";
        services.AddHttpClient<ICalculationApiService, CalculationApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });
        services.AddHttpClient<IOcrService, OcrService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        return services;
    }
    
    public static IServiceCollection AddUserApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthStateService, AuthStateService>();
        services.AddTransient<AuthorizationHandler>();
        
        const string baseApiUrl = "http://localhost:5230";      // TODO : setup all APIs IP in the configuration
        
        services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });
        
        services.AddHttpClient<ICalculationHistoryApiService, CalculationHistoryApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();
        
        services.AddHttpClient<ISavedInputApiService, SavedInputApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();
        
        services.AddHttpClient<ISavedFileApiService, SavedFileApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        return services;
    }
}