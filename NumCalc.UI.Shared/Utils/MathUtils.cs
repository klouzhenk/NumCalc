using System.Globalization;

namespace NumCalc.UI.Shared.Utils;

public static class MathUtils
{
    public static string RangeNumber(this double value, double epsilon)
    {
        if (epsilon == 0)
            return value.ToString(CultureInfo.InvariantCulture);
        
        var decimals = (int)Math.Ceiling(-Math.Log10(Math.Abs(epsilon)));
        decimals = Math.Max(decimals, 0);

        return value.ToString($"F{decimals}", CultureInfo.InvariantCulture);
    }
}