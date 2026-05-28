using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportConnect.Services.Interfaces;

namespace SportConnect.UI.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentSession _session;

    public LoginViewModel(IAuthService auth, ICurrentSession session)
    {
        _auth = auth;
        _session = session;
    }

    [ObservableProperty]
    private string _usernameOrEmail = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>Disparado cuando el login funciona, para que la View cambie a MainWindow.</summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>Disparado cuando el usuario quiere ir a la pantalla de registro.</summary>
    public event EventHandler? RegisterRequested;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var user = await _auth.LoginAsync(UsernameOrEmail, Password);
            if (user is null)
            {
                ErrorMessage = "Invalid username/email or password.";
                return;
            }
            _session.SetUser(user);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
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
    private void GoToRegister() => RegisterRequested?.Invoke(this, EventArgs.Empty);
}
