namespace NumCalc.UI.Shared.Services.Interfaces;

public interface ICultureService
{
    string CurrentCulture { get; }
    Task SetCulture(string culture);
}