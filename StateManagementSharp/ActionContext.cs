using System;
using System.Threading.Tasks;
using StateManagementSharp.Exceptions;

namespace StateManagementSharp
{
    //QA-AS-2018-11-18
    public class ActionContext<TS, TR> where TS : State where TR : RootState
    {
        #region auto-properties

        protected ModuleBase<TS, TR> Module { get; }

        #endregion

        #region ctor(s)

        //QA-AS-2018-11-18
        public ActionContext(ModuleBase<TS, TR> module, TR rootState)
        {
            Module = module;
            RootState = rootState;
        }

        #endregion

        #region access methods

        public TS State => Module.State ?? throw new MissingStateException("ActionState");

        public TR RootState { get; }

        //QA-AS-2018-11-18
        public void Commit<TM, TP>(TP payload) where TM : Mutation<TS, TP>
        {
            //throws
            Module.Commit<TM, TP>(payload);
        }

        //QA-AS-2018-11-18
        public void Commit(string mutationName, object? payload)
        {
            //throws
            Module.Commit(mutationName, payload);
        }

        //QA-AS-2018-11-18
        public async Task Dispatch<TA>(object? payload) where TA : Action<TS, TR>
        {
            //throws
            await Module.Dispatch<TA>(payload);
        }

        public async Task Dispatch<A>() where A : Action<TS, TR>
        {
            //throws
            await Module.Dispatch<A>(null);
        }

        public async Task Dispatch(string actionName, object payload)
        {
            //throws
            await Module.Dispatch(actionName, payload);
        }

        public async Task Dispatch(string actionName)
        {
            //throws
            await Module.Dispatch(actionName);
        }

        #endregion

    }
}
