namespace StateManagementSharp.Sample.Maui.Store;

public class ClearProfileMutation : Mutation<ProfileState, object?>
{
    public ProfileState Apply(ProfileState state, object? payload)
    {
        return state.Clear();
    }
}
