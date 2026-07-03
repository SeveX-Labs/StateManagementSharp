namespace StateManagementSharp.Sample.Maui.Store;

public class SetApplicationTitleMutation : IMutation<AppContextState, string>
{
    public AppContextState Apply(AppContextState state, string applicationTitle)
    {
        return state.WithApplicationTitle(applicationTitle);
    }
}
