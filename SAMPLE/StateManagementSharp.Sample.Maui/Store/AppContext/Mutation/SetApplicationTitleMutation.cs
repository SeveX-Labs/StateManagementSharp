namespace StateManagementSharp.Sample.Maui.Store;

public class SetApplicationTitleMutation : Mutation<AppContextState, string>
{
    public AppContextState Apply(AppContextState state, string applicationTitle)
    {
        return state.WithApplicationTitle(applicationTitle);
    }
}
