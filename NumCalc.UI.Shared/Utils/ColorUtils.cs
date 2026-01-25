using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Utils;

public static class ColorUtils
{
    private static readonly Dictionary<Color, string> CssVariables = new()
    {
        [Color.Primary] = "var(--primary-color)",
        [Color.PrimaryDark] = "var(--primary-dark-color)",
        [Color.PrimaryLight] = "var(--primary-light-color)",

        [Color.Secondary] = "var(--secondary-color)",
        [Color.SecondaryDark] = "var(--secondary-dark-color)",
        [Color.SecondaryLight] = "var(--secondary-light-color)",

        [Color.Success] = "var(--success-color)",
        [Color.SuccessDark] = "var(--success-dark-color)",
        [Color.SuccessLight] = "var(--success-light-color)",

        [Color.Danger] = "var(--danger-color)",
        [Color.DangerDark] = "var(--danger-dark-color)",
        [Color.DangerLight] = "var(--danger-light-color)",

        [Color.Warning] = "var(--warning-color)",
        [Color.WarningDark] = "var(--warning-dark-color)",
        [Color.WarningLight] = "var(--warning-light-color)",
        
        [Color.Black] = "var(--black-color)",
        [Color.White] = "var(--white-color)",
        
        [Color.Gray] = "var(--gray-color)",
        [Color.GrayLight] = "var(--gray-light-color)",
        [Color.GrayUltraLight] = "var(--gray-ultra-light-color)"
    };

    public static readonly Color[] SeriesColors =
    [
        Color.Primary,
        Color.Secondary,
        Color.Success,
        Color.Warning,
        Color.Danger
    ];

    public static string GetColor(Color color) => CssVariables[color]; 

    public static string GetSeriesColor(int index)
    {
        var color = SeriesColors[index % SeriesColors.Length];
        return GetColor(color);
    }
}