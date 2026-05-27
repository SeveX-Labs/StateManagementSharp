namespace StateManagementSharp
{
    public interface MutationFactory
    {
        Mutation? CreateMutation<M>() where M : Mutation;
    }
}