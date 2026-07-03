using System;

namespace StateManagementSharp
{
    public class StateFactoryActivator : StateFactory
    {
        #region StateFactory implementation

        public S? CreateState<S>() where S : State
        {
            return (S?)Activator.CreateInstance(typeof(S), true);
        }

        #endregion
    }
}
