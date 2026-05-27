namespace StateManagementSharp
{
    public interface StateFactory
    {
        S? CreateState<S>() where S : State;
    }
}
