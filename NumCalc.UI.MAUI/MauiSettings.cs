namespace NumCalc.UI.MAUI;

public static class MauiSettings
{
    // Set these to your public API base URLs before publishing.
    // Android emulator note: 10.0.2.2 maps to the host machine's localhost.
#if ANDROID
    public const string CalculationApiBaseUrl = "https://numcalc-calc-api.happymeadow-0b917d17.polandcentral.azurecontainerapps.io";
    public const string UserApiBaseUrl = "https://numcalc-user-api.happymeadow-0b917d17.polandcentral.azurecontainerapps.io";
#else
    public const string CalculationApiBaseUrl = "https://numcalc-calc-api.happymeadow-0b917d17.polandcentral.azurecontainerapps.io";
    public const string UserApiBaseUrl = "https://numcalc-user-api.happymeadow-0b917d17.polandcentral.azurecontainerapps.io";
#endif
}
