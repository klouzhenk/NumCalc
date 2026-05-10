using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Auth;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class AccountSettings : AuthorizedPage<AccountSettings>
{
    [Inject] private IUserApiService UserApiService { get; set; } = null!;
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;

    private AccountSettingsFormModel FormModel { get; init; } = new();
    private ConfirmPasswordFormModel SaveConfirmForm { get; init; } = new();
    private ConfirmPasswordFormModel DeleteConfirmForm { get; init; } = new();

    private BaseModal _saveModal = null!;
    private BaseModal _deleteModal = null!;

    private string _originalUsername = string.Empty;
    private string _originalEmail = string.Empty;

    private bool IsUsernameDirty => FormModel.Username != _originalUsername;
    private bool IsEmailDirty => FormModel.Email != _originalEmail;
    private bool IsPasswordDirty => !string.IsNullOrEmpty(FormModel.NewPassword);
    private bool HasChanges => IsUsernameDirty || IsEmailDirty || IsPasswordDirty;

    protected override async Task OnAuthenticatedInitAsync()
    {
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        await SafeExecuteAsync(async () =>
        {
            var profile = await UserApiService.GetCurrentUserAsync();
            if (profile is null) return;

            ApplyProfile(profile);
        });
    }

    private void ApplyProfile(UserProfileDto profile)
    {
        _originalUsername = profile.Username;
        _originalEmail = profile.Email;
        FormModel.Username = profile.Username;
        FormModel.Email = profile.Email;
    }

    private void ResetUsername() => FormModel.Username = _originalUsername;
    private void ResetEmail() => FormModel.Email = _originalEmail;

    private void ResetPassword()
    {
        FormModel.NewPassword = string.Empty;
        FormModel.ConfirmNewPassword = string.Empty;
    }

    private void OnSaveSubmit()
    {
        SaveConfirmForm.CurrentPassword = string.Empty;
        _saveModal.Show();
    }

    private async Task OnSaveConfirm()
    {
        var success = false;

        await SafeExecuteAsync(async () =>
        {
            var request = new UpdateProfileRequest
            {
                Username = IsUsernameDirty ? FormModel.Username : null,
                Email = IsEmailDirty ? FormModel.Email : null,
                NewPassword = IsPasswordDirty ? FormModel.NewPassword : null,
                CurrentPassword = SaveConfirmForm.CurrentPassword
            };

            await UserApiService.UpdateProfileAsync(request);

            _originalUsername = FormModel.Username;
            _originalEmail = FormModel.Email;
            FormModel.NewPassword = string.Empty;
            FormModel.ConfirmNewPassword = string.Empty;
            SaveConfirmForm.CurrentPassword = string.Empty;
            success = true;
        });

        if (success)
            await _saveModal.Close();
    }

    private void OpenDeleteModal()
    {
        DeleteConfirmForm.CurrentPassword = string.Empty;
        _deleteModal.Show();
    }

    private async Task OnDeleteConfirm()
    {
        await SafeExecuteAsync(async () =>
        {
            await UserApiService.DeleteAccountAsync(new DeleteAccountRequest
            {
                CurrentPassword = DeleteConfirmForm.CurrentPassword
            });

            await TokenStorage.ClearAsync();
            AuthStateService.ClearAuth();
            Navigation.NavigateTo("/");
        });
    }
}
