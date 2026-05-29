using System.Windows;
using System.Windows.Controls;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
        {
            vm.LogoutRequested        += OnLogoutRequested;
            vm.ChangePasswordRequested += OnChangePasswordRequested;
            vm.LoadProfileCommand.Execute(null);
        }
    }

    private void OnLogoutRequested(object? sender, EventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    private void OnChangePasswordRequested(object? sender, EventArgs e)
    {
        if (DataContext is not ProfileViewModel vm) return;

        var oldPwBox  = new PasswordBox { Margin = new Thickness(0, 0, 0, 8) };
        var newPwBox  = new PasswordBox { Margin = new Thickness(0, 0, 0, 8) };
        var confPwBox = new PasswordBox { Margin = new Thickness(0, 0, 0, 16) };

        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock { Text = "Current password",      Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(oldPwBox);
        panel.Children.Add(new TextBlock { Text = "New password",          Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(newPwBox);
        panel.Children.Add(new TextBlock { Text = "Confirm new password",  Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(confPwBox);

        var saveBtn = new Button
        {
            Content = "Change password",
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var win = new Window
        {
            Title = "Change Password",
            Width = 360, SizeToContent = SizeToContent.Height,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            Content = panel
        };

        saveBtn.Click += async (_, _) =>
        {
            if (newPwBox.Password != confPwBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await vm.DoChangePasswordAsync(oldPwBox.Password, newPwBox.Password);
                MessageBox.Show("Password changed successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                win.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        panel.Children.Add(saveBtn);
        win.ShowDialog();
    }
}
