using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Auth;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class ForgotPassword : BasePage<ForgotPassword>
{
    [Inject] private IAuthApiService AuthApiService { get; set; } = null!;

    private ForgotPasswordFormModel ForgotPasswordModel { get; init; } = new();
    private bool Submitted { get; set; }

    private async Task OnForgotPasswordSubmit()
    {
        await SafeExecuteAsync(async () =>
        {
            var request = new ForgotPasswordRequest { Email = ForgotPasswordModel.Email };
            await AuthApiService.ForgotPasswordAsync(request);
            Submitted = true;
        });
    }
}