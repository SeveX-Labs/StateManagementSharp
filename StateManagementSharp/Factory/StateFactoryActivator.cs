using System;

namespace StateManagementSharp
{
    //QA-AS-2018-11-18
    public class StateFactoryActivator : StateFactory
    {
        #region StateFactory implementation

        //QA-AS-2018-11-18
        //throws
        public S? CreateState<S>() where S : State
        {
            //throws
            return (S?)Activator.CreateInstance(typeof(S), true);
        }

        #endregion
    }
}
