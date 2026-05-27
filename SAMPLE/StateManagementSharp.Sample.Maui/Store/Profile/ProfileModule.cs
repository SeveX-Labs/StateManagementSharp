namespace StateManagementSharp.Sample.Maui.Store;

public class ProfileModule : ModuleBase<ProfileState, InternalState>
{
    public ProfileModule(InternalStore store, MutationFactory mutationFactory, ActionFactory actionFactory)
        : base(store, mutationFactory, actionFactory)
    {
    }

    public override void CreateState()
    {
        State = new ProfileState(null, null, null, false);
    }

    public override void DisposeState()
    {
    }
}
