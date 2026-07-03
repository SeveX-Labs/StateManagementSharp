using ProfileContext = StateManagementSharp.ActionContext<StateManagementSharp.Sample.Maui.Store.ProfileState, StateManagementSharp.Sample.Maui.Store.InternalState>;

namespace StateManagementSharp.Sample.Maui.Store;

public class LoadProfileAction : IAction<ProfileState, InternalState>
{
    public async Task Execute(ProfileContext context, object? payload)
    {
        await Task.Delay(250);

        var profile = new SetProfileMutation.ProfileData(
            FirstName: "Mario",
            LastName: "Rossi",
            Email: "mario.rossi@example.com");

        context.Commit(nameof(SetProfileMutation), profile);
    }
}
