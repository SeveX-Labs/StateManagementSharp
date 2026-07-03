# StateManagementSharp.Maui

`StateManagementSharp.Maui` is the .NET MAUI integration package for [`StateManagementSharp`](https://www.nuget.org/packages/StateManagementSharp). It adds MAUI-friendly binding helpers on top of the UI-agnostic core store: `StoreBindableObject`, `BindableValue<T>`, and `UseStateManagementSharp<TStore>()`.

## What this package adds

- **`UseStateManagementSharp<TStore>()`** — a `MauiAppBuilder` extension that registers the StateManagementSharp core services and your store in one call.
- **`StoreBindableObject`** — a base class for reactive MAUI view models that project store state into bindable values.
- **`BindableValue<T>`** — a bindable holder you expose to XAML; assigning its `Value` raises `PropertyChanged`.
- **Automatic UI-thread marshaling** for values bound with `Bind(...)`, so a state change committed on any thread reaches the UI safely.
- **Fody integration, internal to the package**, that weaves `INotifyPropertyChanged` into `BindableValue<T>` — you never reference Fody yourself.

## Relationship with StateManagementSharp

- `StateManagementSharp.Maui` **depends on** the core package `StateManagementSharp`.
- The core stays **UI-agnostic** and provides the store, modules, actions, mutations, the `StateChanged` event, and `Observe(...)`.
- This package **does not replace** the core — it integrates with it, adding the MAUI binding layer.
- Installing `StateManagementSharp.Maui` **brings `StateManagementSharp` in transitively**, so you don't install the core separately.

## Installation

For a .NET MAUI app:

```bash
dotnet add package StateManagementSharp.Maui --version 2.0.0
```

You don't need to install `StateManagementSharp` as well — it is a dependency of this package and is restored automatically.

## Recommended MAUI setup

```csharp
builder
    .UseMauiApp<App>()
    .UseStateManagementSharp<InternalStore>();
```

`UseStateManagementSharp<TStore>()`:

- registers the StateManagementSharp core services;
- scans an assembly for actions, defaulting to `typeof(TStore).Assembly`;
- registers `TStore` as a singleton;
- replaces the manual `builder.Services.AddStateManagementSharp(...)` setup.

Do not also call `builder.Services.AddStateManagementSharp(...)` for the same store in the same `MauiProgram`; `UseStateManagementSharp<TStore>()` already performs the core registration. (Pass explicit assemblies — `UseStateManagementSharp<InternalStore>(extraAssembly)` — only when your actions live outside the store's assembly.)

## ViewModel usage

Derive the view model from `StoreBindableObject` and expose store selectors as `BindableValue<T>` properties:

```csharp
public sealed class MainPageViewModel : StoreBindableObject
{
    public MainPageViewModel(InternalStore store, IDispatcher dispatcher)
        : base(dispatcher)
    {
        store.InitializeStore();

        FirstName = Bind(store.ProfileModule, state => state.FirstName);
        IsBusy = Bind(store.AppContextModule, state => state.IsBusy);

        LoadProfileCommand = new Command(async () =>
            await store.ProfileModule.Dispatch<LoadProfileAction>());
    }

    public BindableValue<string?> FirstName { get; }
    public BindableValue<bool> IsBusy { get; }
    public ICommand LoadProfileCommand { get; }
}
```

Commands only dispatch — there is no manual refresh. `Bind(...)` recomputes each value after every commit and pushes it to the UI thread for you.

## XAML binding

Bind the `.Value` path of each `BindableValue<T>`:

```xml
<Label Text="{Binding FirstName.Value}" />
<ActivityIndicator IsRunning="{Binding IsBusy.Value}" />
```

## Bind vs Observe

- **`Bind(selector)`** — for values the **UI displays** (raw or derived). Returns a `BindableValue<T>`, recomputed on every commit and **marshaled to the UI thread** for you.
- **`Observe(selector, onChanged)`** — for **side-effects / reactions** (logging, navigation, analytics, refreshing non-bound data). It runs **on the thread that committed**, so marshal yourself (for example via `IDispatcher`) if you touch the UI. Both are distinct-until-changed, so identical values don't fire.

## Core-only setup

If you are not building a MAUI app, install the core package instead:

```bash
dotnet add package StateManagementSharp --version 2.0.0
```

and register it manually:

```csharp
builder.Services.AddStateManagementSharp(typeof(InternalStore).Assembly);
builder.Services.AddSingleton<InternalStore>();
```

---

See the [project README on GitHub](https://github.com/SeveX-Labs/StateManagementSharp#readme) for the full core documentation, the reactive model (`StateChanged` / `Observe`), and the 1.x → 2.0 migration guide.
