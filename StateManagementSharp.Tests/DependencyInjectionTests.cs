using Microsoft.Extensions.DependencyInjection;
using StateManagementSharp;
using StateManagementSharp.Extensions;
using Xunit;

namespace StateManagementSharp.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddStateManagementSharp_registers_factories_and_concrete_actions_only()
    {
        var services = new ServiceCollection();

        services.AddStateManagementSharp(typeof(TestStore).Assembly);

        AssertContainsService<MutationFactory>(services);
        AssertContainsService<StateFactory>(services);
        AssertContainsService<ActionFactory>(services);
        AssertContainsTransient<TestAction>(services);
        AssertContainsTransient<DependencyAction>(services);

        AssertDoesNotContainService<AbstractAction>(services);
        Assert.DoesNotContain(services, descriptor =>
            descriptor.ServiceType.IsGenericTypeDefinition &&
            descriptor.ServiceType == typeof(GenericAction<>));
        AssertDoesNotContainService<TestStore>(services);
        AssertDoesNotContainService<TestRootState>(services);
        AssertDoesNotContainService<TestState>(services);
        AssertDoesNotContainService<TestModule>(services);
        AssertDoesNotContainService<ModuleBase<TestState, TestRootState>>(services);
    }

    [Fact]
    public async Task ActionFactory_resolves_actions_through_Microsoft_DI()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new TestDependency("resolved-from-di"));
        services.AddStateManagementSharp(typeof(TestStore).Assembly);

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ActionFactory>();

        var action = factory.CreateAction<DependencyAction>();

        var dependencyAction = Assert.IsType<DependencyAction>(action);
        Assert.Equal("resolved-from-di", dependencyAction.Dependency.Value);
    }

    private static void AssertContainsService<T>(IEnumerable<ServiceDescriptor> services)
    {
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(T));
    }

    private static void AssertContainsTransient<T>(IEnumerable<ServiceDescriptor> services)
    {
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(T));
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    private static void AssertDoesNotContainService<T>(IEnumerable<ServiceDescriptor> services)
    {
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(T));
    }
}
