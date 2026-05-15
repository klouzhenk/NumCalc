using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace NumCalc.UI.MAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Window is null) return;
        // Let the WebView render under the status bar so the app's background shows through.
        WindowCompat.SetDecorFitsSystemWindows(Window, false);
        Window.SetStatusBarColor(global::Android.Graphics.Color.Transparent);
    }
}
