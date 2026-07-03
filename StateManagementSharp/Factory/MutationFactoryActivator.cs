using System;

namespace StateManagementSharp
{
    public class MutationFactoryActivator : MutationFactory
    {
        public IMutation? CreateMutation<TM>() where TM : IMutation
        {
            return (TM?)Activator.CreateInstance(typeof(TM), true);
        }

    }
}
