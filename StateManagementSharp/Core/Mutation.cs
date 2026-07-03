namespace StateManagementSharp
{
    public interface IMutation
    {

    }

    public interface IMutation<S, P> : IMutation where S : IState
    {
        S Apply(S state, P payload);
    }
}
