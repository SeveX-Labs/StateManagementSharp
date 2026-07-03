# StateManagementSharp

[![NuGet](https://img.shields.io/nuget/v/StateManagementSharp.svg)](https://www.nuget.org/packages/StateManagementSharp)
[![NuGet downloads](https://img.shields.io/nuget/dt/StateManagementSharp.svg)](https://www.nuget.org/packages/StateManagementSharp)
[![CI](https://github.com/SeveX-Labs/StateManagementSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/SeveX-Labs/StateManagementSharp/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

StateManagementSharp is a lightweight, dependency-injection-native library for **centralized, app-wide state** in .NET applications. It organizes shared state into a `Store` composed of feature `Module`s, and changes flow through an explicit **Action → Mutation** pipeline, so state transitions stay predictable and easy to follow.

It is conceptually inspired by store/module/action/mutation architectures (such as Vuex and NGXS), but it is a pure .NET library: no JavaScript runtime, no Vue/Angular dependency, and it builds directly on `Microsoft.Extensions.DependencyInjection`.

## What it is (and what it is not)

**It is** a small, structured container for state that is shared across many parts of an app — the kind of state that does not belong to a single view or view model (session/profile, app context, feature-level domain state) — plus a disciplined way to read and change it.

**It is not** a full reactive UI framework or an Rx pipeline. It does, however, raise change notifications: both `Store` and each `ModuleBase` expose a `StateChanged` event and an `Observe(selector, onChanged)` API (with distinct-until-changed), and the companion **`StateManagementSharp.Maui`** package turns those into automatic, UI-thread-marshaled bindings. The UI layer itself stays yours.

## When to use it

- You have **app-wide or cross-view-model state** and want a single, predictable place for it.
- You want **explicit, traceable state transitions**: actions describe intent, and mutations are the only code that changes state.
- You already use `Microsoft.Extensions.DependencyInjection` and want state management that fits naturally into it.
- You are building **.NET MAUI**, ASP.NET Core, worker, or console apps and want the same model across all of them.

## When NOT to use it

- You only need **per-view-model local state** — the MVVM Toolkit is simpler and sufficient.
- You want a **full reactive framework** (Rx streams, operators, effects) — StateManagementSharp signals *that* state changed and lets you observe selectors, but it does not model event pipelines.
- You need **Redux DevTools, time-travel debugging, or effects middleware** now — consider Fluxor.
- You want **reactive streams** over state — consider ReactiveUI.

## How it compares

| Option | Focus | Relationship to StateManagementSharp |
| --- | --- | --- |
| **CommunityToolkit.Mvvm (MVVM Toolkit)** | Observable properties, commands, and messaging **per view model** | **Complementary, not a replacement.** Keep using it for view-model state and commands; use StateManagementSharp for the shared, app-wide state those view models read from. |
| **Fluxor** | Flux/Redux for .NET, strong in Blazor, with effects and Redux DevTools | More feature-complete and Blazor-proven. StateManagementSharp is smaller, module-oriented, and aimed at the general .NET/MAUI case; it does not (yet) offer DevTools or effects. |
| **ReactiveUI** | Reactive (Rx) MVVM and event pipelines | A different mental model. Choose StateManagementSharp if you want a store/module structure without adopting Rx. |
| **A custom singleton service + DI** | Whatever you hand-roll | StateManagementSharp is essentially a structured version of that pattern: modules, typed `Dispatch`/`Commit`, and a single mutation path. If one shared service is all you need, a plain singleton may be enough; the structure pays off as shared state grows. |

## Using it in .NET MAUI

MAUI apps often need state that outlives a single page and is shared by several view models — session and profile data, an app-context/busy flag, feature state. StateManagementSharp gives that state a single home and a predictable update path, registered through the same DI container MAUI already uses.

With the **`StateManagementSharp.Maui`** package, a view model derives from `StoreBindableObject` and binds selectors to `BindableValue<T>` properties; changes are pushed to the UI thread automatically, with no manual refresh (see the [MAUI example](#net-maui-usage) below).

## Features

- Store-based application state with `Store<TRootState>`.
- Feature modules through `ModuleBase<TState, TRootState>`.
- Asynchronous actions through `Action<TState, TRootState>`.
- State updates through `Mutation<TState, TPayload>`.
- Typed module dispatch with `Dispatch<TAction>()` and typed commits with `Commit<TMutation, TPayload>(payload)`.
- Name-based `Dispatch(actionName)` and `Commit(mutationName, payload)` where supported.
- Change notification via `StateChanged` events on `Store`/`ModuleBase`, plus `Observe(selector, onChanged)` with distinct-until-changed.
- DI setup with `AddStateManagementSharp(params Assembly[])` and assembly scanning for concrete action types.
- First-class .NET MAUI integration via the companion `StateManagementSharp.Maui` package (`UseStateManagementSharp<TStore>`, `StoreBindableObject`, `BindableValue<T>`).
- Runs on .NET MAUI, ASP.NET Core / Web API, worker services, and console apps.
- Built directly on `Microsoft.Extensions.DependencyInjection`.

## Installation

Install the package from NuGet:

```bash
dotnet add package StateManagementSharp --version 2.0.0
```

For .NET MAUI apps, also install the integration package (it depends on the core package):

```bash
dotnet add package StateManagementSharp.Maui --version 2.0.0
```

The core `StateManagementSharp` package multi-targets `netstandard2.0`, `net8.0`, and `net10.0`. `StateManagementSharp.Maui` targets the MAUI platforms (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`). The MAUI sample app in this repository is an example and is not part of either package.

## Migrating from 1.x to 2.0

2.0 is a breaking release. The marker interfaces were renamed to the conventional `I`-prefix, which also removes the long-standing collision between `Action<TState, TRootState>` and `System.Action<T1, T2>` — you no longer need to fully qualify `StateManagementSharp.Action`.

| 1.x | 2.0 |
| --- | --- |
| `State` | `IState` |
| `RootState` | `IRootState` |
| `Action` / `Action<TS, TR>` | `IAction` / `IAction<TS, TR>` |
| `Mutation` / `Mutation<S, P>` | `IMutation` / `IMutation<S, P>` |
| `Module` | `IModule` |
| `StateManagementSharp.Action<...>` (qualified to dodge `System.Action`) | `IAction<...>` (no qualification needed) |

`ActionContext<TS, TR>`, the factory interfaces (`ActionFactory`, `MutationFactory`, `StateFactory`), `Store<TR>`, and `ModuleBase<TS, TR>` keep their names. A whole-word replace of the five marker names in type positions covers almost every project; then drop any `StateManagementSharp.` qualifier in front of `Action`.

New in 2.0: subscribe to `Store.StateChanged` / `ModuleBase.StateChanged`, or call `Observe(selector, onChanged)` for fine-grained updates. In MAUI, add the `StateManagementSharp.Maui` package and derive view models from `StoreBindableObject` instead of hand-writing `INotifyPropertyChanged`.

## Basic Setup

Register StateManagementSharp services and your store in DI:

```csharp
builder.Services.AddStateManagementSharp(typeof(InternalStore).Assembly);
builder.Services.AddSingleton<InternalStore>();
```

`AddStateManagementSharp` registers the library factories and scans the provided assemblies for action implementations that should be resolved from DI.

## Minimal Example

The following example shows the core pattern: define a root state, a module state, a module, a mutation, an action, and a store.

```csharp
using Microsoft.Extensions.DependencyInjection;
using StateManagementSharp;

public sealed class AppState : IRootState
{
    private AppStore Store { get; }

    public CounterState Counter => Store.Counter.State!;

    public AppState(AppStore store)
    {
        Store = store;
    }
}

public sealed record CounterState(int Value) : IState;

public sealed class CounterModule : ModuleBase<CounterState, AppState>
{
    public CounterModule(AppStore store, MutationFactory mutationFactory, ActionFactory actionFactory)
        : base(store, mutationFactory, actionFactory)
    {
    }

    public override void CreateState()
    {
        State = new CounterState(0);
    }

    public override void DisposeState()
    {
        State = null;
    }
}

public sealed class IncrementMutation : IMutation<CounterState, int>
{
    public CounterState Apply(CounterState state, int amount)
    {
        return state with { Value = state.Value + amount };
    }
}

public sealed class IncrementAction : IAction<CounterState, AppState>
{
    public Task Execute(ActionContext<CounterState, AppState> context, object? payload)
    {
        var amount = payload is int value ? value : 1;

        context.Commit<IncrementMutation, int>(amount);

        return Task.CompletedTask;
    }
}

public sealed class AppStore : Store<AppState>
{
    public CounterModule Counter { get; private set; } = null!;

    private MutationFactory MutationFactory { get; }
    private ActionFactory ActionFactory { get; }

    public AppStore(MutationFactory mutationFactory, ActionFactory actionFactory)
    {
        MutationFactory = mutationFactory;
        ActionFactory = actionFactory;
        State = new AppState(this);
    }

    public override void InitializeStore()
    {
        BindModules();
        BootstrapModules();
    }

    protected override void BindModules()
    {
        Counter = new CounterModule(this, MutationFactory, ActionFactory);
        Modules = [Counter];
    }

    public override void InitState()
    {
        State = new AppState(this);
    }
}
```

Use the store through DI:

```csharp
var services = new ServiceCollection();

services.AddStateManagementSharp(typeof(AppStore).Assembly);
services.AddSingleton<AppStore>();

await using var provider = services.BuildServiceProvider();

var store = provider.GetRequiredService<AppStore>();
store.InitializeStore();

await store.Counter.Dispatch<IncrementAction>(5);

var value = store.State?.Counter.Value;
```

## Reacting to state changes

Every commit raises `StateChanged` on the owning module and on the store. For fine-grained updates, `Observe` a selector — it emits the current value immediately, then again only when the projected value changes:

```csharp
store.InitializeStore();

// Fires now with the initial value, then on every distinct change.
using var subscription = store.Counter.Observe(
    state => state.Value,
    value => Console.WriteLine($"Counter is now {value}"));

// Any module commit re-evaluates a store-level selector:
store.StateChanged += (_, _) => Console.WriteLine("Something changed");

await store.Counter.Dispatch<IncrementAction>(5);   // prints "Counter is now 5"
```

Callbacks run synchronously on the thread that committed; UI-thread marshaling is handled by the `StateManagementSharp.Maui` package (below). Dispose the `Observe` subscription to stop receiving updates.

## .NET MAUI Usage

Install `StateManagementSharp.Maui` and register the store on the app builder:

```csharp
builder
    .UseMauiApp<App>()
    .UseStateManagementSharp<InternalStore>();   // factories + action scan + AddSingleton<InternalStore>
```

Derive the view model from `StoreBindableObject` and bind selectors to `BindableValue<T>` properties. Changes are marshaled to the UI thread automatically — no manual refresh, no hand-written `INotifyPropertyChanged`:

```csharp
public sealed class MainPageViewModel : StoreBindableObject
{
    public MainPageViewModel(InternalStore store, IDispatcher dispatcher)
        : base(dispatcher)
    {
        store.InitializeStore();

        FirstName = Bind(store.ProfileModule, s => s.FirstName);
        IsBusy    = Bind(store.AppContextModule, s => s.IsBusy);

        LoadProfileCommand = new Command(async () => await store.ProfileModule.Dispatch<LoadProfileAction>());
    }

    public BindableValue<string?> FirstName { get; }
    public BindableValue<bool> IsBusy { get; }
    public ICommand LoadProfileCommand { get; }
}
```

Bind the `.Value` path in XAML:

```xml
<Label Text="{Binding FirstName.Value}" />
<Label Text="{Binding IsBusy.Value}" />
```

`BindableValue<T>` implements `INotifyPropertyChanged` (woven at compile time by Fody, inside the package), so the view updates whenever the selected value changes.

## Web API And Console Usage

StateManagementSharp uses Microsoft DI, so the setup is the same in ASP.NET Core, worker services, and console apps:

```csharp
services.AddStateManagementSharp(typeof(AppStore).Assembly);
services.AddSingleton<AppStore>();
```

Register the store as a singleton for app-wide state. Scoped stores are also possible when the application owns the lifecycle and state boundaries.

## Sample App

The MAUI sample is located at:

```text
SAMPLE/StateManagementSharp.Sample.Maui
```

It demonstrates:

- `Store/InternalStore.cs` and `Store/InternalState.cs`.
- `AppContext` and `Profile` modules.
- Actions and mutations for initializing app context, toggling busy state, loading a profile, and clearing a profile.
- DI registration in `MauiProgram.cs`.
- Store usage from `MainPageViewModel`.

## Build

```bash
dotnet restore StateManagementSharp.sln
dotnet build StateManagementSharp/StateManagementSharp.csproj -c Release
dotnet build StateManagementSharp.Maui/StateManagementSharp.Maui.csproj -c Release -p:MauiVersion=<installed-maui-version>
dotnet build SAMPLE/StateManagementSharp.Sample.Maui/StateManagementSharp.Sample.Maui.csproj -f net10.0-android -c Debug -p:MauiVersion=<installed-maui-version>
```

Pass `-p:MauiVersion` matching your installed MAUI workload (for example `10.0.20`) when building the MAUI package or sample. Building `StateManagementSharp.Maui` for the iOS and MacCatalyst target frameworks requires macOS.

## Tests

```bash
dotnet test StateManagementSharp.Tests/StateManagementSharp.Tests.csproj -c Release
```

## Requirements

- .NET 8.0 or later, or any target compatible with **.NET Standard 2.0**.
- `Microsoft.Extensions.DependencyInjection`.
- The .NET MAUI workload to build the sample app.

The package multi-targets **`netstandard2.0`, `net8.0`, and `net10.0`**, so it can be referenced from .NET Framework 4.6.2+, modern .NET, and .NET MAUI projects.

## Roadmap

Shipped in 2.0:

- Change notification (`StateChanged` + `Observe`) and automatic, UI-thread-marshaled binding via the `StateManagementSharp.Maui` package (bindable adapters woven with **[Fody](https://github.com/Fody/Fody)**).

Planned, but not available yet:

- Source-generated action/mutation registration to replace runtime reflection (`MakeGenericMethod`), enabling iOS full-AOT/trimming.
- Logging/diagnostics and Redux-style middleware (DevTools).
- Relaxing the same-assembly discovery constraint.

## Known Limitations

- StateManagementSharp is not a complete Vuex or NGXS clone.
- No built-in persistence, time-travel, or DevTools support.
- **iOS full-AOT / trimming is not supported yet.** Name-based `Dispatch`/`Commit` use runtime reflection (`Assembly.GetTypes()`, `MakeGenericMethod`) that fails under full AOT/trimming; the typed `Dispatch<TAction>`/`Commit<TMutation, TPayload>` and `Observe` paths are AOT-safe. The fix (source-generated registration) is planned; the sample keeps `PublishTrimmed`/`RunAOTCompilation` disabled.
- Store lifecycle and scoping depend on the app's DI registration; register the store as a singleton for app-wide state.
- Action and mutation discovery scans the store's own assembly, so the store, its modules, actions, and mutations are expected to live in the same assembly.
- On `netstandard2.0`, using C# `record`/`init`-only state types requires an `IsExternalInit` polyfill in the consumer project (not needed on `net8.0`/`net10.0`).

## License

StateManagementSharp is licensed under the [MIT License](LICENSE).
