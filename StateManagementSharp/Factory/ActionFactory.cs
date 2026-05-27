namespace StateManagementSharp
{
    public interface ActionFactory
    {
        Action? CreateAction<A>() where A : Action;

    }
}
