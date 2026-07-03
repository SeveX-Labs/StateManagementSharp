using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using StateManagementSharp.Maui;
using StateManagementSharp.Sample.Maui.Store;

namespace StateManagementSharp.Sample.Maui.ViewModels;

public class MainPageViewModel : StoreBindableObject
{
    private readonly InternalStore _store;

    public MainPageViewModel(InternalStore store, IDispatcher dispatcher)
        : base(dispatcher)
    {
        _store = store;
        _store.InitializeStore();

        // Bind selectors -> UI-thread-marshaled bindable values. No manual refresh anywhere.
        ApplicationTitle = Bind(store.AppContextModule, s => s.ApplicationTitle);
        IsBusy = Bind(store.AppContextModule, s => s.IsBusy);
        FirstName = Bind(store.ProfileModule, s => s.FirstName);
        LastName = Bind(store.ProfileModule, s => s.LastName);
        Email = Bind(store.ProfileModule, s => s.Email);
        IsProfileLoaded = Bind(store.ProfileModule, s => s.IsLoaded);

        InitializeCommand = new Command(async () => await _store.AppContextModule.Dispatch<InitializeAppContextAction>());
        ToggleBusyCommand = new Command(async () => await _store.AppContextModule.Dispatch<ToggleBusyAction>());
        LoadProfileCommand = new Command(async () => await _store.ProfileModule.Dispatch<LoadProfileAction>());
        ClearProfileCommand = new Command(async () => await _store.ProfileModule.Dispatch<ClearProfileAction>());
    }

    public BindableValue<string> ApplicationTitle { get; }
    public BindableValue<bool> IsBusy { get; }
    public BindableValue<string?> FirstName { get; }
    public BindableValue<string?> LastName { get; }
    public BindableValue<string?> Email { get; }
    public BindableValue<bool> IsProfileLoaded { get; }

    public ICommand InitializeCommand { get; }
    public ICommand ToggleBusyCommand { get; }
    public ICommand LoadProfileCommand { get; }
    public ICommand ClearProfileCommand { get; }
}
