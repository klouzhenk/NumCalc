using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Auth;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class ResetPassword : BasePage<ResetPassword>
{
    [Inject] private IAuthApiService AuthApiService { get; set; } = null!;

    private ResetPasswordFormModel ResetPasswordModel { get; init; } = new();
    private string Token { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        
        if (!QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var token) || string.IsNullOrWhiteSpace(token)) 
            return;

        Token = token.ToString();
    }
    
    private async Task OnResetPasswordSubmit()
    {
        await SafeExecuteAsync(async () =>
        {
            var request = new ResetPasswordRequest
            {
                Token = Token,
                NewPassword = ResetPasswordModel.NewPassword
            };
            await AuthApiService.ResetPasswordAsync(request);
            Navigation.NavigateTo("/login");
        });
    }
}