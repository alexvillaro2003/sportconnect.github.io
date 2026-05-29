using System.Windows;
using System.Windows.Controls;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class AdminView : UserControl
{
    public AdminView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdminViewModel vm)
            vm.LoadUsersCommand.Execute(null);
    }
}
