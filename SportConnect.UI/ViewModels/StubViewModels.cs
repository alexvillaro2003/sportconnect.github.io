using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SportConnect.UI.ViewModels;

/// <summary>
/// VM raíz de MainWindow. Aloja los VMs de cada pestaña/zona y expone
/// el usuario actual para binding directo en la vista.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly ICurrentSession _session;

    public MainViewModel(
        ICurrentSession session,
        ActivityFeedViewModel feed,
        MyActivitiesViewModel mine,
        ProfileViewModel profile,
        AdminViewModel admin)
    {
        _session = session;
        Feed = feed;
        MyActivities = mine;
        Profile = profile;
        Admin = admin;

        profile.ProfilePictureChanged += (_, _) => OnPropertyChanged(nameof(CurrentUserPhoto));
    }

    public ActivityFeedViewModel Feed { get; }
    public MyActivitiesViewModel MyActivities { get; }
    public ProfileViewModel Profile { get; }
    public AdminViewModel Admin { get; }

    public string? CurrentUsername => _session.CurrentUser?.Username;
    public bool IsAdmin => _session.IsAdmin;

    public BitmapImage? CurrentUserPhoto =>
        ImageHelpers.ToBitmapImage(_session.CurrentUser?.ProfilePicture);

    public event EventHandler? LogoutRequested;

    [RelayCommand]
    private void Logout()
    {
        _session.Clear();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        switch (SelectedTabIndex)
        {
            case 0: await Feed.LoadFeedCommand.ExecuteAsync(null); break;
            case 1: await MyActivities.LoadMyActivitiesCommand.ExecuteAsync(null); break;
            case 2: await Profile.LoadProfileCommand.ExecuteAsync(null); break;
            case 3: await Admin.LoadUsersCommand.ExecuteAsync(null); break;
        }
    }

    [ObservableProperty]
    private int _selectedTabIndex;

    partial void OnSelectedTabIndexChanged(int value)
    {
        if (value == 1) MyActivities.LoadMyActivitiesCommand.Execute(null);
        if (value == 2) Profile.LoadProfileCommand.Execute(null);
        if (value == 3) Admin.LoadUsersCommand.Execute(null);
    }
}

// ---- Stubs para Sprint 2/3. Mantienen la solución compilable desde el día 1. ----

public partial class MyActivitiesViewModel : ViewModelBase
{
    private readonly IActivityService _activities;
    private readonly ICurrentSession _session;

    public MyActivitiesViewModel(IActivityService activities, ICurrentSession session)
    {
        _activities = activities;
        _session = session;
    }

    public event EventHandler<Activity>? ActivitySelected;

    public ObservableCollection<Activity> MyActivities { get; } = new();

    [RelayCommand]
    private void OpenDetails(Activity activity) => ActivitySelected?.Invoke(this, activity);

    [RelayCommand]
    private async Task LoadMyActivitiesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var items = await _activities.GetUserActivitiesAsync(_session.CurrentUser!.Id);
            MyActivities.Clear();
            foreach (var a in items)
                MyActivities.Add(a);
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

public partial class ProfileViewModel : ViewModelBase
{
    private readonly IUserService _users;
    private readonly IStatsService _stats;
    private readonly IAchievementService _achievements;
    private readonly IAuthService _auth;
    private readonly ICurrentSession _session;

    // Bytes staged by UploadPhoto, committed on Save; null means use session photo
    private byte[]? _profilePictureBytes;

    public event EventHandler? LogoutRequested;
    public event EventHandler? ChangePasswordRequested;
    public event EventHandler? ProfilePictureChanged;

    public ProfileViewModel(
        IUserService users, IStatsService stats,
        IAchievementService achievements, IAuthService auth, ICurrentSession session)
    {
        _users = users;
        _stats = stats;
        _achievements = achievements;
        _auth = auth;
        _session = session;
    }

    public string Username => _session.CurrentUser?.Username ?? string.Empty;
    public string Email    => _session.CurrentUser?.Email    ?? string.Empty;

    [ObservableProperty] private string _bio         = string.Empty;
    [ObservableProperty] private string _nationality = string.Empty;

    [ObservableProperty] private int    _activitiesCreated;
    [ObservableProperty] private int    _activitiesJoined;
    [ObservableProperty] private string _mostPlayedSport = "-";

    // Computed: pending bytes take priority; falls back to last-saved session photo
    public BitmapImage? ProfilePicture =>
        _profilePictureBytes is not null
            ? ImageHelpers.ToBitmapImage(_profilePictureBytes)
            : ImageHelpers.ToBitmapImage(_session.CurrentUser?.ProfilePicture);

    public ObservableCollection<Achievement> Achievements { get; } = new();

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var userId = _session.CurrentUser!.Id;
            await _achievements.EvaluateForUserAsync(userId);

            var stats    = await _stats.ComputeAsync(userId);
            var unlocked = await _achievements.GetUnlockedAsync(userId);
            var user     = _session.CurrentUser!;

            ActivitiesCreated = stats.TotalActivitiesCreated;
            ActivitiesJoined  = stats.TotalActivitiesJoined;
            MostPlayedSport   = stats.MostPlayedSport ?? "-";

            Bio         = user.Bio         ?? string.Empty;
            Nationality = user.Nationality ?? string.Empty;

            _profilePictureBytes = null;
            OnPropertyChanged(nameof(ProfilePicture));

            Achievements.Clear();
            foreach (var a in unlocked)
                Achievements.Add(a);
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
    private async Task SaveProfileAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var bio         = Bio.Length > 0 ? Bio : null;
            var nationality = Nationality.Length > 0 ? Nationality : null;

            await _users.UpdateProfileAsync(_session.CurrentUser!.Id, bio, nationality, _profilePictureBytes);

            // Sync session so next load/refresh sees fresh values without re-querying DB
            _session.CurrentUser!.Bio         = bio;
            _session.CurrentUser!.Nationality = nationality;
            if (_profilePictureBytes is not null)
                _session.CurrentUser!.ProfilePicture = _profilePictureBytes;

            _profilePictureBytes = null;
            ProfilePictureChanged?.Invoke(this, EventArgs.Empty);
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
    private async Task UploadPhotoAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Images|*.jpg;*.jpeg;*.png",
            Title  = "Select profile photo"
        };
        if (dlg.ShowDialog() != true) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            // Stage bytes locally — persisted only when SaveProfile is called
            _profilePictureBytes = await File.ReadAllBytesAsync(dlg.FileName);
            OnPropertyChanged(nameof(ProfilePicture));
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
    private void ChangePassword() => ChangePasswordRequested?.Invoke(this, EventArgs.Empty);

    public async Task DoChangePasswordAsync(string oldPw, string newPw)
        => await _auth.ChangePasswordAsync(_session.CurrentUser!.Id, oldPw, newPw);

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var result = MessageBox.Show(
            "Are you sure? This cannot be undone.",
            "Delete account",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _users.DeleteAccountAsync(_session.CurrentUser!.Id);
            _session.Clear();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
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

public partial class AdminViewModel : ViewModelBase
{
    private readonly IUserService _users;
    private readonly ICurrentSession _session;

    public AdminViewModel(IUserService users, ICurrentSession session)
    {
        _users = users;
        _session = session;
    }

    public ObservableCollection<User> Users        { get; } = new();
    public ObservableCollection<User> FilteredUsers { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        FilteredUsers.Clear();
        var q = value.Trim().ToLower();
        foreach (var u in Users)
            if (string.IsNullOrEmpty(q) ||
                u.Username.ToLower().Contains(q) ||
                u.Email.ToLower().Contains(q))
                FilteredUsers.Add(u);
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var items = await _users.GetAllAsync();
            Users.Clear();
            foreach (var u in items) Users.Add(u);
            OnSearchQueryChanged(SearchQuery);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User user)
    {
        if (user.Id == _session.CurrentUser!.Id)
        {
            MessageBox.Show("You cannot delete your own account.",
                "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Delete {user.Username}? This cannot be undone.",
            "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _users.DeleteAccountAsync(user.Id);
            await LoadUsersAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; IsBusy = false; }
    }

    [RelayCommand]
    private async Task ToggleRoleAsync(User user)
    {
        if (user.Id == _session.CurrentUser!.Id)
        {
            MessageBox.Show("You cannot change your own role.",
                "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var newRole = user.Role == UserRole.Admin ? UserRole.User : UserRole.Admin;

        IsBusy = true;
        try
        {
            await _users.UpdateRoleAsync(user.Id, newRole);
            await LoadUsersAsync();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; IsBusy = false; }
    }
}

internal static class ImageHelpers
{
    public static BitmapImage? ToBitmapImage(byte[]? bytes)
    {
        if (bytes is null) return null;
        using var ms = new MemoryStream(bytes);
        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.StreamSource = ms;
        img.EndInit();
        img.Freeze();
        return img;
    }
}
