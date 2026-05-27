using ProfileContext = StateManagementSharp.ActionContext<StateManagementSharp.Sample.Maui.Store.ProfileState, StateManagementSharp.Sample.Maui.Store.InternalState>;

namespace StateManagementSharp.Sample.Maui.Store;

public class ClearProfileAction : StateManagementSharp.Action<ProfileState, InternalState>
{
    public Task Execute(ProfileContext context, object? payload)
    {
        context.Commit(nameof(ClearProfileMutation), null);

        return Task.CompletedTask;
    }
}
