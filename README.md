# StateManagementSharp

StateManagementSharp is a lightweight state management library for .NET applications. It provides a store-based model with modules, actions, and mutations, built on top of `Microsoft.Extensions.DependencyInjection`.

The design is conceptually inspired by Vuex and NGXS, but it is a .NET library and does not depend on Vue, Vuex, or a JavaScript runtime.

## Features

- Store-based application state with `Store<TRootState>`.
- Feature modules through `ModuleBase<TState, TRootState>`.
- Asynchronous actions through `Action<TState, TRootState>`.
- State updates through `Mutation<TState, TPayload>`.
- Typed module dispatch with `Dispatch<TAction>()`.
- Typed module commits with `Commit<TMutation, TPayload>(payload)`.
- Name-based `Dispatch(actionName)` and `Commit(mutationName, payload)` where supported.
- DI setup with `AddStateManagementSharp(params Assembly[])`.
- Assembly scanning for concrete, non-abstract, closed action types.
- Works in .NET MAUI, Web API, console apps, and other .NET projects.
- Uses Microsoft DI directly.

## Installation

Install the package from NuGet:

```bash
dotnet add package StateManagementSharp --version 1.0.0
```

The NuGet package contains the core `StateManagementSharp` library. The MAUI sample app is included in this repository as an example and is not included in the package.

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

A plain ViewModel can receive the store from DI, initialize it, dispatch actions, and read the current state:

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

- .NET 10.
- `Microsoft.Extensions.DependencyInjection`.
- .NET MAUI workload for the sample app.

## Known Limitations

- StateManagementSharp is not a complete Vuex or NGXS clone.
- There is no built-in persistence, time travel, or devtools support in v1.
- Store lifecycle and scoping depend on the app's DI registration.
- Singleton stores are suitable for app-wide state; scoped stores are possible when registered and owned by the app.

## License

StateManagementSharp is licensed under the [MIT License](LICENSE).
