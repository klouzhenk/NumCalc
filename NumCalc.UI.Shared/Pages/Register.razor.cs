using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Auth;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class Register : BasePage<Register>
{
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private IAuthApiService AuthApiService { get; set; } = null!;
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;

    private RegisterFormModel RegisterForm { get; init; } = new();

    private async Task OnRegistrationSubmit()
    {
        await SafeExecuteAsync(async () =>
        {
            var response = await AuthApiService.RegisterAsync(new RegisterRequest
            {
                Username = RegisterForm.Username,
                Password = RegisterForm.Password
            });

            if (response is null) return;

            AuthStateService.SetAuth(response);
            await TokenStorage.SaveAsync(response.Token, response.Username);
            Navigation.NavigateTo("/");
        });
    }
}
