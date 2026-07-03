using System;

namespace StateManagementSharp
{
    public class MutationFactoryActivator : MutationFactory
    {
        public Mutation? CreateMutation<TM>() where TM : Mutation
        {
            return (TM?)Activator.CreateInstance(typeof(TM), true);
        }

    }
}
