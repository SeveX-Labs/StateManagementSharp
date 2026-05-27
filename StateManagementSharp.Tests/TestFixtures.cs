using StateManagementSharp;

namespace StateManagementSharp.Tests;

internal sealed record TestDependency(string Value);

internal sealed class TestRootState : RootState
{
}

internal sealed class TestState : State
{
    public int DispatchCount { get; set; }
    public string? LastPayload { get; set; }
    public string? Value { get; set; }
    public string? NamedValue { get; set; }
}

internal sealed class TestStore : Store<TestRootState>
{
    public TestModule Module { get; }

    private TestStore(MutationFactory mutationFactory, ActionFactory actionFactory)
    {
        State = new TestRootState();
        Module = new TestModule(this, mutationFactory, actionFactory);
        Modules = new[] { Module };
        Module.CreateState();
    }

    public static TestStore Create()
    {
        return Create(new MutationFactoryActivator(), new ActionFactoryActivator());
    }

    public static TestStore Create(MutationFactory mutationFactory, ActionFactory actionFactory)
    {
        return new TestStore(mutationFactory, actionFactory);
    }

    public override void InitState()
    {
        State = new TestRootState();
    }

    public override void InitializeStore()
    {
    }

    protected override void BindModules()
    {
    }
}

internal sealed class TestModule : ModuleBase<TestState, TestRootState>
{
    public TestModule(Store<TestRootState> rootStore, MutationFactory mutationFactory, ActionFactory actionFactory)
        : base(rootStore, mutationFactory, actionFactory)
    {
    }

    public override void DisposeState()
    {
        State = null;
    }

    public override void CreateState()
    {
        State = new TestState();
    }
}

internal sealed class TestAction : StateManagementSharp.Action<TestState, TestRootState>
{
    public Task Execute(ActionContext<TestState, TestRootState> context, object? payload)
    {
        context.State.DispatchCount++;
        context.State.LastPayload = payload as string;

        return Task.CompletedTask;
    }
}

internal sealed class DependencyAction(TestDependency dependency) : StateManagementSharp.Action<TestState, TestRootState>
{
    public TestDependency Dependency { get; } = dependency;

    public Task Execute(ActionContext<TestState, TestRootState> context, object? payload)
    {
        return Task.CompletedTask;
    }
}

internal abstract class AbstractAction : StateManagementSharp.Action<TestState, TestRootState>
{
    public abstract Task Execute(ActionContext<TestState, TestRootState> context, object? payload);
}

internal sealed class GenericAction<T> : StateManagementSharp.Action<TestState, TestRootState>
{
    public Task Execute(ActionContext<TestState, TestRootState> context, object? payload)
    {
        return Task.CompletedTask;
    }
}

internal sealed class SetValueMutation : Mutation<TestState, string>
{
    public TestState Apply(TestState state, string payload)
    {
        state.Value = payload;
        return state;
    }
}

internal sealed class NamedMutation : Mutation<TestState, string>
{
    public TestState Apply(TestState state, string payload)
    {
        state.NamedValue = payload;
        return state;
    }
}

internal sealed class ExplodingMutation : Mutation<TestState, string>
{
    public TestState Apply(TestState state, string payload)
    {
        throw new InvalidOperationException("boom");
    }
}
