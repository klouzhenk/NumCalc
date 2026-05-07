namespace NumCalc.UI.Shared.Components;

public partial class RedirectToLogin
{
    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/login", forceLoad: false, replace: true);
    }
}