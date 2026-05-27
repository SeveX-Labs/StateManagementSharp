namespace StateManagementSharp.Sample.Maui.Store;

public class InternalStore : Store<InternalState>
{
    public AppContextModule AppContextModule { get; private set; } = null!;
    public ProfileModule ProfileModule { get; private set; } = null!;

    private bool _initialized;

    private MutationFactory MutationFactory { get; }
    private ActionFactory ActionFactory { get; }

    public InternalStore(MutationFactory mutationFactory, ActionFactory actionFactory)
    {
        MutationFactory = mutationFactory;
        ActionFactory = actionFactory;

        State = new InternalState(this);
    }

    public override void InitializeStore()
    {
        if (_initialized)
        {
            return;
        }

        BindModules();
        BootstrapModules();

        _initialized = true;
    }

    protected override void BindModules()
    {
        AppContextModule = new AppContextModule(this, MutationFactory, ActionFactory);
        ProfileModule = new ProfileModule(this, MutationFactory, ActionFactory);

        Modules = [AppContextModule, ProfileModule];
    }

    public override void InitState()
    {
        if (Modules is not null && Modules.Any())
        {
            foreach (var module in Modules)
            {
                module.CreateState();
            }
        }

        State = new InternalState(this);
    }
}
