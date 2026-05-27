using Microsoft.Extensions.DependencyInjection;
using StateManagementSharp;
using StateManagementSharp.Exceptions;
using Xunit;

namespace StateManagementSharp.Tests;

public sealed class StoreBehaviorTests
{
    [Fact]
    public async Task Module_dispatch_executes_action()
    {
        var store = TestStore.Create();

        await store.Module.Dispatch<TestAction>("dispatched-payload");

        Assert.Equal(1, store.Module.State?.DispatchCount);
        Assert.Equal("dispatched-payload", store.Module.State?.LastPayload);
    }

    [Fact]
    public void Module_commit_typed_applies_mutation()
    {
        var store = TestStore.Create();

        store.Module.Commit<SetValueMutation, string>("typed-value");

        Assert.Equal("typed-value", store.Module.State?.Value);
    }

    [Fact]
    public void Module_commit_by_name_uses_concrete_mutation_name()
    {
        var store = TestStore.Create();

        store.Module.Commit<NamedMutation>("named-value");

        Assert.Equal("named-value", store.Module.State?.NamedValue);
    }

    [Fact]
    public void Store_commit_wraps_runtime_failures_as_CommitFailedException()
    {
        var store = TestStore.Create();

        var exception = Assert.Throws<CommitFailedException>(() => store.Commit(nameof(ExplodingMutation), "payload"));

        Assert.NotNull(exception.OriginalException);
        Assert.True(
            exception.OriginalException is InvalidOperationException ||
            exception.OriginalException.InnerException is InvalidOperationException);
    }

    [Fact]
    public async Task Mutations_are_not_resolved_from_DI()
    {
        var services = new ServiceCollection();
        services.AddStateManagementSharp(typeof(TestStore).Assembly);

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(SetValueMutation));

        await using var provider = services.BuildServiceProvider();
        var store = TestStore.Create(
            provider.GetRequiredService<MutationFactory>(),
            provider.GetRequiredService<ActionFactory>());

        store.Module.Commit<SetValueMutation, string>("created-by-activator");

        Assert.Equal("created-by-activator", store.Module.State?.Value);
    }
}
