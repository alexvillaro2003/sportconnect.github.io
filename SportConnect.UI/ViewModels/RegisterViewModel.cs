using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportConnect.Services.Interfaces;

namespace SportConnect.UI.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentSession _session;

    public RegisterViewModel(IAuthService auth, ICurrentSession session)
    {
        _auth = auth;
        _session = session;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public event EventHandler? RegistrationSucceeded;
    public event EventHandler? BackToLoginRequested;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;
        try
        {
            var user = await _auth.RegisterAsync(Username, Email, Password);
            _session.SetUser(user);
            RegistrationSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void BackToLogin() => BackToLoginRequested?.Invoke(this, EventArgs.Empty);
}
