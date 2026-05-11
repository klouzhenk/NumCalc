using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var baseApiUrl = configuration["Apis:CalculationApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: Apis:CalculationApi:BaseUrl");

        services.AddHttpClient<ICalculationApiService, CalculationApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        return services;
    }

    public static IServiceCollection AddUserApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationCore();
        services.AddScoped<IAuthStateService, AuthStateService>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        var baseApiUrl = configuration["Apis:UserApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration: Apis:UserApi:BaseUrl");

        services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        services.AddHttpClient<IUserApiService, UserApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        services.AddHttpClient<ICalculationHistoryApiService, CalculationHistoryApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        services.AddHttpClient<ISavedInputApiService, SavedInputApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        services.AddHttpClient<ISavedFileApiService, SavedFileApiService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
        });

        return services;
    }
}