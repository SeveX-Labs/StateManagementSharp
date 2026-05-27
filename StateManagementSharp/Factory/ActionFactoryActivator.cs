using System;

namespace StateManagementSharp
{
    //QA-AS-2018-11-18
    public class ActionFactoryActivator : ActionFactory
    {
        #region ctor(s)

        //QA-AS-2018-11-18
        //throws
        public Action? CreateAction<TA>() where TA : Action
        {
            //QA-AS-2018-11-18
            //throws
            return (TA?)Activator.CreateInstance(typeof(TA), true);
        }


        #endregion
    }
}
