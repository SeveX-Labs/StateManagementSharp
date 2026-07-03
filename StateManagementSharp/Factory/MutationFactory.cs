namespace StateManagementSharp
{
    public interface MutationFactory
    {
        IMutation? CreateMutation<M>() where M : IMutation;
    }
}