using System;

namespace StateManagementSharp
{
    public class ActionFactoryActivator : ActionFactory
    {
        #region ctor(s)

        public IAction? CreateAction<TA>() where TA : IAction
        {
            return (TA?)Activator.CreateInstance(typeof(TA), true);
        }


        #endregion
    }
}
