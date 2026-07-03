namespace StateManagementSharp.Sample.Maui.Store;

public class InternalState : IRootState
{
    public InternalStore Store { get; }

    public AppContextState AppContextState => Store.AppContextModule.State;
    public ProfileState ProfileState => Store.ProfileModule.State;

    public InternalState(InternalStore store)
    {
        Store = store;
    }
}
