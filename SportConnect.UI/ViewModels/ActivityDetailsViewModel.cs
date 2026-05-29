using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;
using System.Collections.ObjectModel;

namespace SportConnect.UI.ViewModels;

public partial class ActivityDetailsViewModel : ViewModelBase
{
    private readonly IActivityService _activities;
    private readonly IChatService _chatService;
    private readonly ICurrentSession _session;

    public event EventHandler? ActionCompleted;
    public event EventHandler? Cancelled;
    public event EventHandler<Activity>? EditRequested;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JoinedParticipants))]
    [NotifyPropertyChangedFor(nameof(SpotsText))]
    [NotifyPropertyChangedFor(nameof(CanManage))]
    [NotifyPropertyChangedFor(nameof(CanJoin))]
    private Activity? _activity;

    public bool CanManage =>
        Activity != null &&
        (_session.CurrentUser?.Id == Activity.Creator.Id || _session.IsAdmin);

    public bool CanJoin =>
        Activity != null &&
        !IsParticipant &&
        !Activity.IsFull;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanJoin))]
    private bool _isParticipant;

    [ObservableProperty]
    private ObservableCollection<Message> _messages = new();

    [ObservableProperty]
    private string _newMessageContent = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChatClosed))]
    private bool _isChatActive;

    public bool IsChatClosed => !IsChatActive;

    public IEnumerable<ActivityParticipant> JoinedParticipants =>
        Activity?.Participants.Where(p => p.Status == ParticipantStatus.Joined) ?? [];

    public string SpotsText =>
        Activity != null ? $"{JoinedParticipants.Count()}/{Activity.MaxPlayers}" : "-";

    public ActivityDetailsViewModel(IActivityService activities, IChatService chatService, ICurrentSession session)
    {
        _activities = activities;
        _chatService = chatService;
        _session = session;
    }

    public async Task LoadAsync(int activityId)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Activity = await _activities.GetByIdAsync(activityId);
            if (Activity != null)
            {
                var userId = _session.CurrentUser!.Id;
                IsParticipant = Activity.Participants.Any(
                    p => p.UserId == userId && p.Status == ParticipantStatus.Joined);
                await LoadMessagesCommand.ExecuteAsync(null);
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

    [RelayCommand]
    private async Task LoadMessagesAsync()
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(Activity!.Id);
            Messages.Clear();
            foreach (var m in messages)
                Messages.Add(m);
            IsChatActive = await _chatService.IsChatActiveAsync(Activity.Id);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMessageContent))
            return;

        IsBusy = true;
        try
        {
            var message = await _chatService.SendMessageAsync(
                Activity!.Id,
                _session.CurrentUser!.Id,
                NewMessageContent.Trim());

            Messages.Add(message);
            NewMessageContent = string.Empty;
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
    private async Task JoinAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _activities.JoinAsync(Activity!.Id, _session.CurrentUser!.Id);
            ActionCompleted?.Invoke(this, EventArgs.Empty);
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
    private async Task LeaveAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _activities.LeaveAsync(Activity!.Id, _session.CurrentUser!.Id);
            ActionCompleted?.Invoke(this, EventArgs.Empty);
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
    private void OpenEdit() => EditRequested?.Invoke(this, Activity!);

    [RelayCommand]
    private async Task DeleteAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _activities.DeleteAsync(Activity!.Id, _session.CurrentUser!.Id);
            ActionCompleted?.Invoke(this, EventArgs.Empty);
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
    private void CloseDialog() => Cancelled?.Invoke(this, EventArgs.Empty);
}
