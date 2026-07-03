namespace StateManagementSharp
{
    public interface ActionFactory
    {
        IAction? CreateAction<A>() where A : IAction;

    }
}
