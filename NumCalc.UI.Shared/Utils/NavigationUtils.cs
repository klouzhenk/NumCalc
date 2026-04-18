using NumCalc.UI.Shared.Enums;

namespace NumCalc.UI.Shared.Utils;

public static class NavigationUtils
{
    public static Dictionary<NavigationItem, string> NavigationItems { get; } = new()
    {
        { NavigationItem.Roots, "root-finding" },
        { NavigationItem.EquationSystems, "equation-systems" },
        { NavigationItem.Integration, "integration" },
        { NavigationItem.Interpolation, "interpolation" },
        { NavigationItem.Differentiation, "differentiation" },
        { NavigationItem.Optimization, "optimization" },
        { NavigationItem.Ode, "ode" },
    };
}