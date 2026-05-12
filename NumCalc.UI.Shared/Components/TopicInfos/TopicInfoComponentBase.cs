using System.Globalization;
using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components.TopicInfos;

public abstract class TopicInfoComponentBase : ComponentBase, IDisposable
{
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    protected abstract NavigationItem Item { get; }
    
    protected TopicInfo? TopicInfo;

    protected override void OnInitialized()
    {
        UiStateService.OnTopicInfoRequested += OnTopicInfoRequested;
    }

    private void OnTopicInfoRequested(NavigationItem item)
    {
        if (item != Item) return;
        
        TopicInfo?.Show();
        InvokeAsync(StateHasChanged);
    }

    protected static string Localize(string en, string uk) => IsUk ? uk : en;

    private static bool IsUk => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "uk";

    public void Dispose()
    {
        UiStateService.OnTopicInfoRequested -= OnTopicInfoRequested;
    }
}
