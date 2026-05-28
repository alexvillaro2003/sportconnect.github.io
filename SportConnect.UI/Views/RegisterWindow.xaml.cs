using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class RegisterWindow : Window
{
    private readonly RegisterViewModel _vm;

    public RegisterWindow(RegisterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;

        _vm.RegistrationSucceeded += OnRegistrationSucceeded;
        _vm.BackToLoginRequested += OnBackToLoginRequested;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb) _vm.Password = pb.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb) _vm.ConfirmPassword = pb.Password;
    }

    private void OnRegistrationSucceeded(object? sender, EventArgs e)
    {
        var main = App.Services.GetRequiredService<MainWindow>();
        main.Show();
        Close();
    }

    private void OnBackToLoginRequested(object? sender, EventArgs e)
    {
        var login = App.Services.GetRequiredService<LoginWindow>();
        login.Show();
        Close();
    }
}
