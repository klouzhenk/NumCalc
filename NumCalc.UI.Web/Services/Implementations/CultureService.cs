using System.Globalization;
using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Services.Interfaces;

namespace WebUI.Services.Implementations;

public class CultureService(NavigationManager navigationManager) : ICultureService
{
    public string CurrentCulture => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    
    public Task SetCulture(string culture)
    {
        var uri = new Uri(navigationManager.Uri);
        var redirectUri = uri.PathAndQuery;

        navigationManager.NavigateTo(
            $"/set-culture?culture={culture}&redirectUri={Uri.EscapeDataString
                (redirectUri)}",
            forceLoad: true
        );

        return Task.CompletedTask;
    }
}