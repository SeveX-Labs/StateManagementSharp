namespace StateManagementSharp.Sample.Maui.Store;

public class AppContextModule : ModuleBase<AppContextState, InternalState>
{
    public AppContextModule(InternalStore store, MutationFactory mutationFactory, ActionFactory actionFactory)
        : base(store, mutationFactory, actionFactory)
    {
    }

    public override void CreateState()
    {
        State = new AppContextState("Not initialized", false);
    }

    public override void DisposeState()
    {
    }
}
