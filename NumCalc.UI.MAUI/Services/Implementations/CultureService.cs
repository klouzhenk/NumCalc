using System.Globalization;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.MAUI.Services.Implementations;

public class CultureService : ICultureService
{
    private const string PreferenceKey = "app_culture";
    
    public string CurrentCulture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    
    public Task SetCulture(string culture)
    {
        Preferences.Set(PreferenceKey, culture);

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        return Task.CompletedTask;
    }
}