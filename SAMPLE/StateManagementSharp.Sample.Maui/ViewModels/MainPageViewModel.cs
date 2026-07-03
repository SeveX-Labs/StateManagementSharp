using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using StateManagementSharp.Maui;
using StateManagementSharp.Sample.Maui.Store;

namespace StateManagementSharp.Sample.Maui.ViewModels;

// This view model shows the two reactive tools added in StateManagementSharp 2.0:
//
//   Bind(...)    -> values the UI DISPLAYS (raw or derived). A BindableValue<T> that
//                   recomputes after every commit and is marshaled onto the UI thread
//                   for you by StoreBindableObject.
//   Observe(...) -> REACTIONS to state changes (side-effects / non-visual logic). It runs
//                   on the thread that committed the mutation, so YOU marshal to the UI.
//
// Commands only dispatch. There is no RefreshFromStore(): after a dispatch the bound values
// and the reaction log update themselves.  Flow:
//   Button -> Dispatch -> Action/Mutation -> StateChanged -> Bind/Observe -> UI
public class MainPageViewModel : StoreBindableObject
{
    private const int MaxLogEntries = 50;

    private readonly InternalStore _store;
    private readonly IDispatcher _dispatcher;

    // Observe returns an IDisposable. We keep them so a longer-lived host could dispose them;
    // this single-page sample's view model lives for the app's lifetime.
    private readonly List<IDisposable> _reactions = new();

    private int _nameChangeCount;

    public MainPageViewModel(InternalStore store, IDispatcher dispatcher)
        : base(dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
        _store.InitializeStore();

        // ---------- Bind: values displayed by the UI (auto-marshaled, recomputed on commit) ----------

        ApplicationTitle = Bind(store.AppContextModule, s => s.ApplicationTitle);
        IsBusy = Bind(store.AppContextModule, s => s.IsBusy);

        // Direct field projections.
        Email = Bind(store.ProfileModule, s => s.Email ?? "—");
        HasProfile = Bind(store.ProfileModule, s => s.IsLoaded);

        // Derived values computed INSIDE Bind — there is no such field in the state.
        ProfileName = Bind(store.ProfileModule, s => s.IsLoaded ? $"{s.FirstName} {s.LastName}" : "—");
        ProfileSummary = Bind(store.ProfileModule, s => s.IsLoaded ? $"{s.FirstName} {s.LastName} <{s.Email}>" : "No profile loaded");

        // A derived flag that drives UI enablement (the Clear button).
        CanClearProfile = Bind(store.ProfileModule, s => s.IsLoaded);

        // Derived from the ROOT state, so it recomputes when ANY module commits.
        StatusText = Bind(store, s => s.AppContextState.IsBusy ? "Working…"
                                    : s.ProfileState.IsLoaded ? "Ready"
                                    : "Idle");

        // ---------- Observe: react to state changes with side-effects (write to a visible log) ----------

        _reactions.Add(store.AppContextModule.Observe(
            s => s.IsBusy,
            busy => AppendLog(busy ? "⏳ busy started" : "✅ busy ended")));

        _reactions.Add(store.ProfileModule.Observe(
            s => s.IsLoaded,
            loaded => AppendLog(loaded ? "👤 profile loaded" : "🗑️ profile cleared")));

        // distinct-until-changed: the sample always loads the same "Mario Rossi", so tapping
        // Load Profile repeatedly changes nothing — this reaction fires only ONCE.
        _reactions.Add(store.ProfileModule.Observe(
            s => s.FirstName,
            name => AppendLog($"✏️ name → '{name ?? "—"}' (change #{++_nameChangeCount})")));

        // ---------- Commands: dispatch only, never touch UI state directly ----------

        InitializeCommand = new Command(async () => await _store.AppContextModule.Dispatch<InitializeAppContextAction>());
        ToggleBusyCommand = new Command(async () => await _store.AppContextModule.Dispatch<ToggleBusyAction>());
        LoadProfileCommand = new Command(async () => await _store.ProfileModule.Dispatch<LoadProfileAction>());
        ClearProfileCommand = new Command(async () => await _store.ProfileModule.Dispatch<ClearProfileAction>());
        ClearLogCommand = new Command(() => ReactionLog.Clear());
    }

    // --- Bound values (Bind). BindableValue<T> raises PropertyChanged on .Value. ---
    public BindableValue<string> ApplicationTitle { get; }
    public BindableValue<bool> IsBusy { get; }
    public BindableValue<string> StatusText { get; }
    public BindableValue<string> ProfileSummary { get; }
    public BindableValue<string> ProfileName { get; }
    public BindableValue<string> Email { get; }
    public BindableValue<bool> HasProfile { get; }
    public BindableValue<bool> CanClearProfile { get; }

    // --- Reaction log, written only by Observe callbacks. Self-notifying, so no INPC needed. ---
    public ObservableCollection<string> ReactionLog { get; } = new();

    // --- Commands ---
    public ICommand InitializeCommand { get; }
    public ICommand ToggleBusyCommand { get; }
    public ICommand LoadProfileCommand { get; }
    public ICommand ClearProfileCommand { get; }
    public ICommand ClearLogCommand { get; }

    // Observe callbacks run on the thread that committed the mutation. Unlike Bind, they are NOT
    // marshaled for you, so hop onto the UI thread before touching the bound ObservableCollection.
    private void AppendLog(string line)
    {
        void Insert()
        {
            ReactionLog.Insert(0, line);
            if (ReactionLog.Count > MaxLogEntries)
                ReactionLog.RemoveAt(ReactionLog.Count - 1);
        }

        if (_dispatcher.IsDispatchRequired)
            _dispatcher.Dispatch(Insert);
        else
            Insert();
    }
}
