namespace StateManagementSharp.Sample.Maui.Store;

public class ClearProfileMutation : IMutation<ProfileState, object?>
{
    public ProfileState Apply(ProfileState state, object? payload)
    {
        return state.Clear();
    }
}
