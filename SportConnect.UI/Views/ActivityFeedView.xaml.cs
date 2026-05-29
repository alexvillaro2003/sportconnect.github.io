using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.Domain.Entities;
using SportConnect.UI.ViewModels;

namespace SportConnect.UI.Views;

public partial class ActivityFeedView : UserControl
{
    public ActivityFeedView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ActivityFeedViewModel vm)
        {
            vm.CreateActivityRequested += OnCreateActivityRequested;
            vm.ActivityDetailsRequested += OnActivityDetailsRequested;
            vm.LoadFeedCommand.Execute(null);
        }
    }

    private void OnCreateActivityRequested(object? sender, EventArgs e)
    {
        var dialog = App.Services.GetRequiredService<CreateActivityDialog>();
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true && DataContext is ActivityFeedViewModel vm)
            vm.LoadFeedCommand.Execute(null);
    }

    private async void OnActivityDetailsRequested(object? sender, Activity activity)
    {
        var dialog = App.Services.GetRequiredService<ActivityDetailsDialog>();
        if (dialog.DataContext is ActivityDetailsViewModel vm)
        {
            await vm.LoadAsync(activity.Id);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true && DataContext is ActivityFeedViewModel feedVm)
                feedVm.LoadFeedCommand.Execute(null);
        }
    }
}
