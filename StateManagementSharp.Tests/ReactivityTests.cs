using System.Collections.Generic;
using StateManagementSharp;
using Xunit;

namespace StateManagementSharp.Tests;

public sealed class ReactivityTests
{
    [Fact]
    public void Module_typed_commit_raises_StateChanged()
    {
        var store = TestStore.Create();
        var count = 0;
        store.Module.StateChanged += (_, _) => count++;

        store.Module.Commit<SetValueMutation, string>("value");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Name_based_commit_raises_module_StateChanged()
    {
        var store = TestStore.Create();
        var count = 0;
        store.Module.StateChanged += (_, _) => count++;

        // Goes through the reflection path: Store.Commit -> MakeGenericMethod -> ModuleBase.Commit<,>
        store.Commit(nameof(SetValueMutation), "value");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Observe_emits_current_value_immediately()
    {
        var store = TestStore.Create();
        store.Module.Commit<SetValueMutation, string>("seed");

        var seen = new List<string?>();
        using var subscription = store.Module.Observe(s => s.Value, v => seen.Add(v));

        Assert.Equal(new string?[] { "seed" }, seen);
    }

    [Fact]
    public void Observe_emits_on_each_distinct_change()
    {
        var store = TestStore.Create();

        var seen = new List<string?>();
        using var subscription = store.Module.Observe(s => s.Value, v => seen.Add(v));

        store.Module.Commit<SetValueMutation, string>("first");
        store.Module.Commit<SetValueMutation, string>("second");

        Assert.Equal(new string?[] { null, "first", "second" }, seen);
    }

    [Fact]
    public void Observe_suppresses_unchanged_projected_value()
    {
        var store = TestStore.Create();
        store.Module.Commit<SetValueMutation, string>("same");

        var seen = new List<string?>();
        using var subscription = store.Module.Observe(s => s.Value, v => seen.Add(v));

        store.Module.Commit<SetValueMutation, string>("same");     // projected value unchanged -> suppressed
        store.Module.Commit<SetValueMutation, string>("changed");

        Assert.Equal(new string?[] { "same", "changed" }, seen);
    }

    [Fact]
    public void Observe_stops_after_dispose()
    {
        var store = TestStore.Create();

        var seen = new List<string?>();
        var subscription = store.Module.Observe(s => s.Value, v => seen.Add(v));
        subscription.Dispose();

        store.Module.Commit<SetValueMutation, string>("after-dispose");

        Assert.Equal(new string?[] { null }, seen);               // only the initial emit
    }

    [Fact]
    public void Store_aggregates_StateChanged_from_modules()
    {
        var store = TestStore.Create();
        store.BootstrapModules();                                 // wires module -> store aggregation

        var count = 0;
        store.StateChanged += (_, _) => count++;

        store.Module.Commit<SetValueMutation, string>("value");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Store_bootstrap_wiring_is_idempotent()
    {
        var store = TestStore.Create();
        store.BootstrapModules();
        store.BootstrapModules();                                 // must not double-subscribe

        var count = 0;
        store.StateChanged += (_, _) => count++;

        store.Module.Commit<SetValueMutation, string>("value");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Store_observe_reacts_to_module_commit()
    {
        var store = TestStore.Create();
        store.BootstrapModules();

        var seen = new List<string?>();
        using var subscription = store.Observe(_ => store.Module.State?.Value, v => seen.Add(v));

        store.Module.Commit<SetValueMutation, string>("value");

        Assert.Equal(new string?[] { null, "value" }, seen);
    }
}
