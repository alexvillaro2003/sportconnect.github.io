using System.Windows;
using SportConnect.Domain.Entities;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class EditActivityDialog : Window
{
    private readonly EditActivityViewModel _vm;

    public EditActivityDialog(EditActivityViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.ActivityUpdated += (_, _) => { DialogResult = true; Close(); };
        vm.Cancelled += (_, _) => { DialogResult = false; Close(); };
    }

    public async Task LoadActivity(Activity activity) => await _vm.LoadAsync(activity);
}
