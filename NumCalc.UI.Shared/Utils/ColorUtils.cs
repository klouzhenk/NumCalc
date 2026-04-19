using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Utils;

public static class ColorUtils
{
    // Hex approximations of the CSS color-mix() values (base: primary #8C2155, secondary #3F826D, black #0D1321, white #F4F3EE)
    private static readonly Dictionary<Color, string> HexColors = new()
    {
        [Color.Primary]       = "#8C2155",
        [Color.PrimaryDark]   = "#731E4B",
        [Color.PrimaryLight]  = "#F5E4ED",
        [Color.PrimaryBorder] = "#D4A3BC",

        [Color.Secondary]      = "#3F826D",
        [Color.SecondaryDark]  = "#326156",
        [Color.SecondaryLight] = "#A0C4BA",

        [Color.Success]      = "#8EA604",
        [Color.SuccessDark]  = "#728303",
        [Color.SuccessLight] = "#C4D782",

        [Color.Danger]      = "#E71D36",
        [Color.DangerDark]  = "#B9172B",
        [Color.DangerLight] = "#F08E97",

        [Color.Warning]      = "#FEC601",
        [Color.WarningDark]  = "#CB9E01",
        [Color.WarningLight] = "#FEEAA0",

        [Color.Black] = "#0D1321",
        [Color.White] = "#F4F3EE",

        [Color.Gray]          = "#6B6F7A",
        [Color.GrayLight]     = "#9CA3AF",
        [Color.GrayUltraLight] = "#ECEAE5",
    };

    private static readonly Dictionary<Color, string> CssVariables = new()
    {
        [Color.Primary]       = "var(--primary-color)",
        [Color.PrimaryDark]   = "var(--primary-dark-color)",
        [Color.PrimaryLight]  = "var(--primary-light-color)",
        [Color.PrimaryBorder] = "var(--primary-border-color)",

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

    public static string GetHexColor(Color color) => HexColors[color];

    public static string GetSeriesColor(int index)
    {
        var color = SeriesColors[index % SeriesColors.Length];
        return GetColor(color);
    }
}