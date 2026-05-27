using System;
using System.Threading.Tasks;
using StateManagementSharp.Core;
using StateManagementSharp.Exceptions;

namespace StateManagementSharp
{

    //QA-AS-2018-11-18
    public abstract class ModuleBase<TS, TR> : Module where TS : State where TR : RootState
    {
        #region autp-properties

        public TS? State { get; protected set; }

        private Store<TR> RootStore { get; }

        private MutationFactory MutationFactory { get; }
        private ActionFactory ActionFactory { get; }

        #endregion

        #region properties

        private TR? RootState => RootStore.State;

        #endregion

        #region ctor(s)

        //QA-AS-2018-11-18
        protected ModuleBase(Store<TR> rootStore, MutationFactory mutationFactory, ActionFactory actionFactory)
        {
            RootStore = rootStore;
            MutationFactory = mutationFactory;
            ActionFactory = actionFactory;
        }

        #endregion

        #region access methods


        //QA-AS-2018-11-18
        //throws
        public void Commit<TM, TP>(TP payload) where TM : Mutation<TS, TP>
        {
            if (State is null) throw new MissingStateException(nameof(State));

            if (MutationFactory.CreateMutation<TM>() is Mutation<TS, TP> mutation)
                State = mutation.Apply(State, payload);
        }

        //QA-AS-2018-11-18
        //throws
        public async Task Dispatch<TA>() where TA : Action<TS, TR>
        {
            await Dispatch<TA>(null);
        }

        //QA-AS-2018-11-18
        //throws
        public async Task Dispatch<TA>(object? payload) where TA : Action<TS, TR>
        {
            if (RootState is null) throw new MissingRootStateException(nameof(RootState));

            if (ActionFactory.CreateAction<TA>() is Action<TS, TR> action)
            {
                var actionContext = new ActionContext<TS, TR>(this, RootState);
                await action.Execute(actionContext, payload);
            }
        }

        //QA-AS-2018-11-18
        //throws
        public virtual void Commit(string mutationName, object? payload)
        {
            RootStore.Commit(mutationName, payload);
        }

        //QA-AS-2018-11-18
        //throws
        public virtual void Commit<TM>(object? payload)
        {
            RootStore.Commit(typeof(TM).Name, payload);
        }

        //QA-AS-2018-11-18
        //throws
        public virtual Task Dispatch(string actionName)
        {
            return RootStore.Dispatch(actionName);
        }

        public virtual Task Dispatch(string actionName, object? payload)
        {
            return RootStore.Dispatch(actionName, payload);
        }

        #endregion

        #region abstract methods

        public abstract void DisposeState();
        public abstract void CreateState();

        #endregion
    }
}
