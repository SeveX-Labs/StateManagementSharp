namespace StateManagementSharp
{
    public interface Mutation
    {

    }

    public interface Mutation<S, P> : Mutation where S : State
    {
        S Apply(S state, P payload);
    }
}
