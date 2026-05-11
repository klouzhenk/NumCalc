using System.Globalization;

namespace NumCalc.UI.Shared.Utils;

public static class MathUtils
{
    private const int DefaultDecimals = 5;
    private const int MaxDecimals = 10;

    public static string FormatResult(this double value, double? tolerance = null)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return value.ToString(CultureInfo.InvariantCulture);

        var decimals = DecimalsFromTolerance(tolerance);
        var rounded = Math.Round(value, decimals);
        var format = "0." + new string('#', decimals);
        return rounded.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string FormatResult(this double? value, double? tolerance = null)
        => value.HasValue ? value.Value.FormatResult(tolerance) : string.Empty;

    public static int DecimalsFromTolerance(double? tolerance)
    {
        if (!tolerance.HasValue || tolerance.Value == 0 || double.IsNaN(tolerance.Value))
            return DefaultDecimals;

        var raw = (int)Math.Ceiling(-Math.Log10(Math.Abs(tolerance.Value)));
        return Math.Clamp(raw, 0, MaxDecimals);
    }
}
