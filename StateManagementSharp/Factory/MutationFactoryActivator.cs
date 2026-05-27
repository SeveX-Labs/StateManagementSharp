using System;

namespace StateManagementSharp
{
    //QA-AS-2018-11-18
    public class MutationFactoryActivator : MutationFactory
    {
        //QA-AS-2018-11-18
        //throws
        public Mutation? CreateMutation<TM>() where TM : Mutation
        {
            //throws
            return (TM?)Activator.CreateInstance(typeof(TM), true);
        }

    }
}
