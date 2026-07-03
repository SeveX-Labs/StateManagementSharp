# StateManagementSharp

[![NuGet](https://img.shields.io/nuget/v/StateManagementSharp.svg)](https://www.nuget.org/packages/StateManagementSharp)
[![NuGet downloads](https://img.shields.io/nuget/dt/StateManagementSharp.svg)](https://www.nuget.org/packages/StateManagementSharp)
[![CI](https://github.com/SeveX-Labs/StateManagementSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/SeveX-Labs/StateManagementSharp/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

StateManagementSharp is a lightweight, dependency-injection-native library for **centralized, app-wide state** in .NET applications. It organizes shared state into a `Store` composed of feature `Module`s, and changes flow through an explicit **Action → Mutation** pipeline, so state transitions stay predictable and easy to follow.

It is conceptually inspired by store/module/action/mutation architectures (such as Vuex and NGXS), but it is a pure .NET library: no JavaScript runtime, no Vue/Angular dependency, and it builds directly on `Microsoft.Extensions.DependencyInjection`.

## What it is (and what it is not)

**It is** a small, structured container for state that is shared across many parts of an app — the kind of state that does not belong to a single view or view model (session/profile, app context, feature-level domain state) — plus a disciplined way to read and change it.

**It is not** a reactive UI framework. In this version, changing state does **not** automatically raise change notifications: after dispatching an action you read the current state (for example, refreshing bindable properties in a view model). Automatic change notification is planned for a future release (see [Roadmap](#roadmap)).

## When to use it

- You have **app-wide or cross-view-model state** and want a single, predictable place for it.
- You want **explicit, traceable state transitions**: actions describe intent, and mutations are the only code that changes state.
- You already use `Microsoft.Extensions.DependencyInjection` and want state management that fits naturally into it.
- You are building **.NET MAUI**, ASP.NET Core, worker, or console apps and want the same model across all of them.

## When NOT to use it

- You only need **per-view-model local state** — the MVVM Toolkit is simpler and sufficient.
- You need **automatic UI updates today** from state changes — that is not implemented yet.
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

Because there is no automatic change notification yet, a view model reads the store after dispatching (see the [MAUI example](#net-maui-usage) below). This keeps the 1.x line honest and simple; reactive binding is on the [roadmap](#roadmap).

## Features

- Store-based application state with `Store<TRootState>`.
- Feature modules through `ModuleBase<TState, TRootState>`.
- Asynchronous actions through `Action<TState, TRootState>`.
- State updates through `Mutation<TState, TPayload>`.
- Typed module dispatch with `Dispatch<TAction>()` and typed commits with `Commit<TMutation, TPayload>(payload)`.
- Name-based `Dispatch(actionName)` and `Commit(mutationName, payload)` where supported.
- DI setup with `AddStateManagementSharp(params Assembly[])` and assembly scanning for concrete action types.
- Runs on .NET MAUI, ASP.NET Core / Web API, worker services, and console apps.
- Built directly on `Microsoft.Extensions.DependencyInjection`.

## Installation

Install the package from NuGet:

```bash
dotnet add package StateManagementSharp --version 1.1.0
```

The NuGet package contains the core `StateManagementSharp` library. The MAUI sample app is included in this repository as an example and is not part of the package.

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

public sealed class AppState : RootState
{
    private AppStore Store { get; }

    public CounterState Counter => Store.Counter.State!;

    public AppState(AppStore store)
    {
        Store = store;
    }
}

public sealed record CounterState(int Value) : State;

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

public sealed class IncrementMutation : Mutation<CounterState, int>
{
    public CounterState Apply(CounterState state, int amount)
    {
        return state with { Value = state.Value + amount };
    }
}

public sealed class IncrementAction : StateManagementSharp.Action<CounterState, AppState>
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

## .NET MAUI Usage

The sample app registers the store in `MauiProgram.cs`:

```csharp
builder.Services.AddStateManagementSharp(typeof(InternalStore).Assembly);
builder.Services.AddSingleton<InternalStore>();
```

A plain ViewModel can receive the store from DI, initialize it, dispatch actions, and read the current state. Note that state changes do not raise notifications yet, so the ViewModel reads the store after each dispatch:

```csharp
public sealed class MainPageViewModel
{
    private readonly InternalStore _store;

    public MainPageViewModel(InternalStore store)
    {
        _store = store;
        _store.InitializeStore();
    }

    public async Task LoadAsync()
    {
        await _store.ProfileModule.Dispatch<LoadProfileAction>();

        // Read current state after dispatch (no automatic change notification yet).
        var profile = _store.State?.ProfileState;
        var firstName = profile?.FirstName;
    }
}
```

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
dotnet build SAMPLE/StateManagementSharp.Sample.Maui/StateManagementSharp.Sample.Maui.csproj -f net10.0-android -c Debug
```

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

Planned, but not available yet:

- Automatic change notification for bindable UI (implemented via **[Fody](https://github.com/Fody/Fody)**), so views update without manual refresh.
- A dedicated `StateManagementSharp.Maui` integration package.
- Logging/diagnostics middleware.

## Known Limitations

- StateManagementSharp is not a complete Vuex or NGXS clone.
- No automatic UI change notification yet: read state after dispatch.
- No built-in persistence, time-travel, or devtools support in the 1.x line.
- Store lifecycle and scoping depend on the app's DI registration; register the store as a singleton for app-wide state.
- Action and mutation discovery scans the store's own assembly, so the store, its modules, actions, and mutations are expected to live in the same assembly.

## License

StateManagementSharp is licensed under the [MIT License](LICENSE).
