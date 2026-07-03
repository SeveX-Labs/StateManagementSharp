using System;

namespace StateManagementSharp
{
    public class StateFactoryActivator : StateFactory
    {
        #region StateFactory implementation

        public S? CreateState<S>() where S : IState
        {
            return (S?)Activator.CreateInstance(typeof(S), true);
        }

        #endregion
    }
}
