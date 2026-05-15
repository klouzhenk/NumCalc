using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NumCalc.UI.MAUI.Services.Implementations;
using NumCalc.UI.Shared.Extensions;
using NumCalc.UI.Shared.Services.Implementations;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Apis:CalculationApi:BaseUrl"] = MauiSettings.CalculationApiBaseUrl,
            ["Apis:UserApi:BaseUrl"] = MauiSettings.UserApiBaseUrl,
        });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddScoped<ICultureService, CultureService>();
        builder.Services.AddScoped<ITokenStorage, MauiTokenStorage>();
        builder.Services.AddScoped<IStorageService, MauiStorageService>();

        builder.Services.AddNumCalcUiSharedServices();
        builder.Services.AddCalculationApiServices(builder.Configuration);
        builder.Services.AddUserApiServices(builder.Configuration);

        var logPath = Path.Combine(FileSystem.AppDataDirectory, "Logs", "maui-log-.txt");
        builder.Services.AddSharedLogging(logPath);

        var savedCulture = Preferences.Get("app_culture", "uk");
        var cultureInfo = new CultureInfo(savedCulture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        return builder.Build();
    }
}
