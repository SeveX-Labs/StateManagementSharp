using System;

namespace StateManagementSharp
{
    public class ActionFactoryActivator : ActionFactory
    {
        #region ctor(s)

        public Action? CreateAction<TA>() where TA : Action
        {
            return (TA?)Activator.CreateInstance(typeof(TA), true);
        }


        #endregion
    }
}
