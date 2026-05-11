using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components.TopicInfos;

public abstract class TopicInfoComponentBase : ComponentBase
{
    protected static string Localize(string en, string uk) => IsUk ? uk : en;

    private static bool IsUk => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "uk";
}
