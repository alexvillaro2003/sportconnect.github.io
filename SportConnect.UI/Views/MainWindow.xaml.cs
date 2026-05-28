using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        void GoToLogin(object? s, EventArgs e)
        {
            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            Close();
        }

        vm.LogoutRequested          += GoToLogin;
        vm.Profile.LogoutRequested  += GoToLogin;
    }
}
