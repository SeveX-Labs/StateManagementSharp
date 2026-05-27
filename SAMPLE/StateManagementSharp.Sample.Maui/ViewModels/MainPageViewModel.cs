using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StateManagementSharp.Sample.Maui.Store;

namespace StateManagementSharp.Sample.Maui.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly InternalStore _store;

    private string _applicationTitle = string.Empty;
    private bool _isBusy;
    private string? _firstName;
    private string? _lastName;
    private string? _email;
    private bool _isProfileLoaded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ApplicationTitle
    {
        get => _applicationTitle;
        private set => SetProperty(ref _applicationTitle, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string? FirstName
    {
        get => _firstName;
        private set => SetProperty(ref _firstName, value);
    }

    public string? LastName
    {
        get => _lastName;
        private set => SetProperty(ref _lastName, value);
    }

    public string? Email
    {
        get => _email;
        private set => SetProperty(ref _email, value);
    }

    public bool IsProfileLoaded
    {
        get => _isProfileLoaded;
        private set => SetProperty(ref _isProfileLoaded, value);
    }

    public ICommand InitializeCommand { get; }
    public ICommand ToggleBusyCommand { get; }
    public ICommand LoadProfileCommand { get; }
    public ICommand ClearProfileCommand { get; }

    public MainPageViewModel(InternalStore store)
    {
        _store = store;
        _store.InitializeStore();

        InitializeCommand = new Command(async () => await InitializeAsync());
        ToggleBusyCommand = new Command(async () => await ToggleBusyAsync());
        LoadProfileCommand = new Command(async () => await LoadProfileAsync());
        ClearProfileCommand = new Command(async () => await ClearProfileAsync());

        RefreshFromStore();
    }

    private async Task InitializeAsync()
    {
        await _store.AppContextModule.Dispatch<InitializeAppContextAction>();
        RefreshFromStore();
    }

    private async Task ToggleBusyAsync()
    {
        await _store.AppContextModule.Dispatch<ToggleBusyAction>();
        RefreshFromStore();
    }

    private async Task LoadProfileAsync()
    {
        await _store.ProfileModule.Dispatch<LoadProfileAction>();
        RefreshFromStore();
    }

    private async Task ClearProfileAsync()
    {
        await _store.ProfileModule.Dispatch<ClearProfileAction>();
        RefreshFromStore();
    }

    private void RefreshFromStore()
    {
        var appContext = _store.State?.AppContextState ?? default;
        var profile = _store.State?.ProfileState ?? default;

        ApplicationTitle = appContext.ApplicationTitle;
        IsBusy = appContext.IsBusy;
        FirstName = profile.FirstName;
        LastName = profile.LastName;
        Email = profile.Email;
        IsProfileLoaded = profile.IsLoaded;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
