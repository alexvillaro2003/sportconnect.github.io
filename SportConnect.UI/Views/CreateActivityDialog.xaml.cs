using System.Windows;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class CreateActivityDialog : Window
{
    public CreateActivityDialog(CreateActivityViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.ActivityCreated += (_, _) => { DialogResult = true; Close(); };
        vm.Cancelled += (_, _) => { DialogResult = false; Close(); };
        Loaded += async (_, _) => await vm.LoadSportsCommand.ExecuteAsync(null);
    }
}
