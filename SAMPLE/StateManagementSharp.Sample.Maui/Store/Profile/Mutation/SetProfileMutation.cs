namespace StateManagementSharp.Sample.Maui.Store;

public class SetProfileMutation : Mutation<ProfileState, SetProfileMutation.ProfileData>
{
    public record ProfileData(string FirstName, string LastName, string Email);

    public ProfileState Apply(ProfileState state, ProfileData profile)
    {
        return state.WithProfile(profile.FirstName, profile.LastName, profile.Email);
    }
}
