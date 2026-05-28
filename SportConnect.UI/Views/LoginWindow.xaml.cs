using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        _vm.LoginSucceeded += OnLoginSucceeded;
        _vm.RegisterRequested += OnRegisterRequested;
    }

    // PasswordBox no soporta binding bidireccional por seguridad, así que
    // sincronizamos manualmente. Es la única violación pragmática del MVVM puro.
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb) _vm.Password = pb.Password;
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        var main = App.Services.GetRequiredService<MainWindow>();
        main.Show();
        Close();
    }

    private void OnRegisterRequested(object? sender, EventArgs e)
    {
        var register = new RegisterWindow(App.Services.GetRequiredService<RegisterViewModel>());
        register.Show();
        Close();
    }
}
