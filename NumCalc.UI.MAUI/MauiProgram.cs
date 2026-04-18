using System.Globalization;
using Microsoft.Extensions.Logging;
using NumCalc.UI.MAUI.Services.Implementations;
using NumCalc.UI.Shared.Extensions;
using NumCalc.UI.Shared.HttpServices.Implementations;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.MAUI;

public static class MauiProgram
{
    private const string BaseApiUrlWindows = "http://localhost:5229";
    private const string BaseApiUrlAndroid = "http://10.0.2.2:5229";
    
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddScoped<ICultureService, CultureService>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddHttpClient<ICalculationApiService, CalculationApiService>(client =>
        {
#if ANDROID
            client.BaseAddress = new Uri(BaseApiUrlAndroid);
#else
            client.BaseAddress = new Uri(BaseApiUrlWindows);
#endif
        });

        var logPath = Path.Combine(FileSystem.AppDataDirectory, "Logs", "maui-log-.txt");
        builder.Services.AddSharedLogging(logPath);

        var savedCulture = Preferences.Get("app_culture", "uk");
        var cultureInfo = new CultureInfo(savedCulture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        return builder.Build();
    }
}