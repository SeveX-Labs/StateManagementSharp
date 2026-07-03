namespace StateManagementSharp.Sample.Maui.Store;

public readonly struct ProfileState : IState
{
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Email { get; }
    public bool IsLoaded { get; }

    public ProfileState(string? firstName, string? lastName, string? email, bool isLoaded)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsLoaded = isLoaded;
    }

    public ProfileState WithProfile(string firstName, string lastName, string email)
    {
        return new ProfileState(firstName, lastName, email, true);
    }

    public ProfileState Clear()
    {
        return new ProfileState(null, null, null, false);
    }
}
