namespace StateManagementSharp.Sample.Maui.Store;

public class SetIsBusyMutation : Mutation<AppContextState, bool>
{
    public AppContextState Apply(AppContextState state, bool isBusy)
    {
        return state.WithIsBusy(isBusy);
    }
}
