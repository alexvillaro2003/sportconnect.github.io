using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;
using System.Collections.ObjectModel;

namespace SportConnect.UI.ViewModels;

public partial class EditActivityViewModel : ViewModelBase
{
    private readonly IActivityService _activities;
    private readonly ICurrentSession _session;

    private int _activityId;

    public event EventHandler? ActivityUpdated;
    public event EventHandler? Cancelled;

    public ObservableCollection<Sport> Sports { get; } = new();

    [ObservableProperty]
    private Sport? _selectedSport;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _maxPlayersText = "2";

    [ObservableProperty]
    private DateTime _scheduledDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private int _scheduledHour = 12;

    [ObservableProperty]
    private int _scheduledMinute = 0;

    public IReadOnlyList<int> Hours { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> Minutes { get; } = Enumerable.Range(0, 12).Select(i => i * 5).ToList();

    public EditActivityViewModel(IActivityService activities, ICurrentSession session)
    {
        _activities = activities;
        _session = session;
    }

    public async Task LoadAsync(Activity activity)
    {
        _activityId = activity.Id;

        var sports = await _activities.GetAllSportsAsync();
        Sports.Clear();
        foreach (var s in sports)
            Sports.Add(s);

        SelectedSport = Sports.FirstOrDefault(s => s.Id == activity.SportId);
        Location = activity.Location;
        Description = activity.Description ?? string.Empty;
        MaxPlayersText = activity.MaxPlayers.ToString();

        ScheduledDate = activity.ScheduledAt.Date;
        ScheduledHour = activity.ScheduledAt.Hour;
        ScheduledMinute = (activity.ScheduledAt.Minute / 5) * 5;

    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedSport is null)
        {
            ErrorMessage = "Please select a sport.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Location))
        {
            ErrorMessage = "Location is required.";
            return;
        }
        if (!int.TryParse(MaxPlayersText, out int maxPlayers) || maxPlayers < 2)
        {
            ErrorMessage = "Max players must be at least 2.";
            return;
        }
        var scheduledAt = ScheduledDate.Date.AddHours(ScheduledHour).AddMinutes(ScheduledMinute);

        IsBusy = true;
        try
        {
            await _activities.UpdateAsync(
                _activityId,
                _session.CurrentUser!.Id,
                SelectedSport.Id,
                scheduledAt,
                maxPlayers,
                ActivityVisibility.Public,
                Location,
                string.IsNullOrWhiteSpace(Description) ? null : Description);

            ActivityUpdated?.Invoke(this, EventArgs.Empty);
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

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
