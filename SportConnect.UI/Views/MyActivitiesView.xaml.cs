using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.Domain.Entities;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class MyActivitiesView : UserControl
{
    public MyActivitiesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MyActivitiesViewModel vm)
        {
            vm.ActivitySelected += OnActivitySelected;
            vm.LoadMyActivitiesCommand.Execute(null);
        }
    }

    private async void OnActivitySelected(object? sender, Activity activity)
    {
        var dialog = App.Services.GetRequiredService<ActivityDetailsDialog>();
        if (dialog.DataContext is ActivityDetailsViewModel vm)
        {
            await vm.LoadAsync(activity.Id);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true && DataContext is MyActivitiesViewModel myVm)
                myVm.LoadMyActivitiesCommand.Execute(null);
        }
    }
}
