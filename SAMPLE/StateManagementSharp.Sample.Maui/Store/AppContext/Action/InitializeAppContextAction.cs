using AppContext = StateManagementSharp.ActionContext<StateManagementSharp.Sample.Maui.Store.AppContextState, StateManagementSharp.Sample.Maui.Store.InternalState>;

namespace StateManagementSharp.Sample.Maui.Store;

public class InitializeAppContextAction : IAction<AppContextState, InternalState>
{
    public Task Execute(AppContext context, object? payload)
    {
        context.Commit(nameof(SetApplicationTitleMutation), "StateManagementSharp MAUI Sample");

        return Task.CompletedTask;
    }
}
