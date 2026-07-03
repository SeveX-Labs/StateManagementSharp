using System;
using System.Threading.Tasks;
using StateManagementSharp.Core;
using StateManagementSharp.Exceptions;

namespace StateManagementSharp
{

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

        protected ModuleBase(Store<TR> rootStore, MutationFactory mutationFactory, ActionFactory actionFactory)
        {
            RootStore = rootStore;
            MutationFactory = mutationFactory;
            ActionFactory = actionFactory;
        }

        #endregion

        #region access methods


        public void Commit<TM, TP>(TP payload) where TM : Mutation<TS, TP>
        {
            if (State is null) throw new MissingStateException(nameof(State));

            if (MutationFactory.CreateMutation<TM>() is Mutation<TS, TP> mutation)
                State = mutation.Apply(State, payload);
        }

        public async Task Dispatch<TA>() where TA : Action<TS, TR>
        {
            await Dispatch<TA>(null);
        }

        public async Task Dispatch<TA>(object? payload) where TA : Action<TS, TR>
        {
            if (RootState is null) throw new MissingRootStateException(nameof(RootState));

            if (ActionFactory.CreateAction<TA>() is Action<TS, TR> action)
            {
                var actionContext = new ActionContext<TS, TR>(this, RootState);
                await action.Execute(actionContext, payload);
            }
        }

        public virtual void Commit(string mutationName, object? payload)
        {
            RootStore.Commit(mutationName, payload);
        }

        public virtual void Commit<TM>(object? payload)
        {
            RootStore.Commit(typeof(TM).Name, payload);
        }

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
