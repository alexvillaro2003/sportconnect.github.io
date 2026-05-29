using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.Domain.Entities;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class ActivityDetailsDialog : Window
{
    private readonly ActivityDetailsViewModel _vm;

    public ActivityDetailsDialog(ActivityDetailsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.ActionCompleted += (_, _) => { DialogResult = true; Close(); };
        vm.Cancelled += (_, _) => { DialogResult = false; Close(); };
        vm.EditRequested += OnEditRequested;
    }

    private async void OnEditRequested(object? sender, Activity activity)
    {
        var dialog = App.Services.GetRequiredService<EditActivityDialog>();
        dialog.Owner = this;
        await dialog.LoadActivity(activity);
        if (dialog.ShowDialog() == true)
            await _vm.LoadAsync(activity.Id);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e) =>
        _vm.OpenEditCommand.Execute(null);

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to delete this activity? This cannot be undone.",
            "Delete Activity",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
            _vm.DeleteCommand.Execute(null);
    }
}
