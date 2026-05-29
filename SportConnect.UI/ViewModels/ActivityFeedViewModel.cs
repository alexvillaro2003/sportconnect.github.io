using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportConnect.Domain.Entities;
using SportConnect.Services.Interfaces;
using System.Collections.ObjectModel;

namespace SportConnect.UI.ViewModels;

public partial class ActivityFeedViewModel : ViewModelBase
{
    private readonly IActivityService _activities;

    private static readonly Sport _allSports = new() { Name = "All sports" };

    public ActivityFeedViewModel(IActivityService activities)
    {
        _activities = activities;
        _selectedSport = _allSports;
        Sports.Add(_allSports);
    }

    public event EventHandler? CreateActivityRequested;
    public event EventHandler<Activity>? ActivityDetailsRequested;

    public ObservableCollection<Activity> Activities { get; } = new();
    public ObservableCollection<Sport> Sports { get; } = new();

    [ObservableProperty]
    private Sport? _selectedSport;

    partial void OnSelectedSportChanged(Sport? value) => LoadFeedCommand.Execute(null);

    [RelayCommand]
    private void OpenCreateActivity() => CreateActivityRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenDetails(Activity activity) => ActivityDetailsRequested?.Invoke(this, activity);

    [RelayCommand]
    private async Task LoadFeedAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            int? sportId = SelectedSport is { Id: > 0 } s ? s.Id : null;
            var items = await _activities.GetFeedAsync(sportId);

            Activities.Clear();
            foreach (var a in items)
                Activities.Add(a);

            if (Sports.Count == 1)
            {
                var allSports = await _activities.GetAllSportsAsync();
                foreach (var sport in allSports)
                    Sports.Add(sport);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
