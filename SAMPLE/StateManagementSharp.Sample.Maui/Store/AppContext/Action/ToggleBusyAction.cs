using AppContext = StateManagementSharp.ActionContext<StateManagementSharp.Sample.Maui.Store.AppContextState, StateManagementSharp.Sample.Maui.Store.InternalState>;

namespace StateManagementSharp.Sample.Maui.Store;

public class ToggleBusyAction : IAction<AppContextState, InternalState>
{
    public Task Execute(AppContext context, object? payload)
    {
        context.Commit(nameof(SetIsBusyMutation), !context.State.IsBusy);

        return Task.CompletedTask;
    }
}
