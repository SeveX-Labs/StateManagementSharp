using System;
using System.Threading.Tasks;
using StateManagementSharp.Exceptions;

namespace StateManagementSharp
{
    public class ActionContext<TS, TR> where TS : IState where TR : IRootState
    {
        #region auto-properties

        protected ModuleBase<TS, TR> Module { get; }

        #endregion

        #region ctor(s)

        public ActionContext(ModuleBase<TS, TR> module, TR rootState)
        {
            Module = module;
            RootState = rootState;
        }

        #endregion

        #region access methods

        public TS State => Module.State ?? throw new MissingStateException("ActionState");

        public TR RootState { get; }

        public void Commit<TM, TP>(TP payload) where TM : IMutation<TS, TP>
        {
            Module.Commit<TM, TP>(payload);
        }

        public void Commit(string mutationName, object? payload)
        {
            Module.Commit(mutationName, payload);
        }

        public async Task Dispatch<TA>(object? payload) where TA : IAction<TS, TR>
        {
            await Module.Dispatch<TA>(payload);
        }

        public async Task Dispatch<A>() where A : IAction<TS, TR>
        {
            await Module.Dispatch<A>(null);
        }

        public async Task Dispatch(string actionName, object payload)
        {
            await Module.Dispatch(actionName, payload);
        }

        public async Task Dispatch(string actionName)
        {
            await Module.Dispatch(actionName);
        }

        #endregion

    }
}
