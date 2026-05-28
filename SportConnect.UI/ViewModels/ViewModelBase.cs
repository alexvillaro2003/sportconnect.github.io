using CommunityToolkit.Mvvm.ComponentModel;

namespace SportConnect.UI.ViewModels;

/// <summary>
/// Base de todos los ViewModels. Hereda de ObservableObject (CommunityToolkit.Mvvm)
/// para ahorrar el boilerplate de INotifyPropertyChanged.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;
}
