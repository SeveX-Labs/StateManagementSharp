using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StateManagementSharp.Core;
using StateManagementSharp.Exceptions;

namespace StateManagementSharp
{

    public abstract class ModuleBase<TS, TR> : IModule where TS : IState where TR : IRootState
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

        #region events

        /// <inheritdoc />
        public event EventHandler? StateChanged;

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


        public void Commit<TM, TP>(TP payload) where TM : IMutation<TS, TP>
        {
            if (State is null) throw new MissingStateException(nameof(State));

            if (MutationFactory.CreateMutation<TM>() is IMutation<TS, TP> mutation)
            {
                State = mutation.Apply(State, payload);
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task Dispatch<TA>() where TA : IAction<TS, TR>
        {
            await Dispatch<TA>(null);
        }

        public async Task Dispatch<TA>(object? payload) where TA : IAction<TS, TR>
        {
            if (RootState is null) throw new MissingRootStateException(nameof(RootState));

            if (ActionFactory.CreateAction<TA>() is IAction<TS, TR> action)
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

        /// <summary>
        /// Observes a projection of this module's state. Invokes <paramref name="onChanged"/> once
        /// immediately with the current value (if state exists), then again after each commit whose
        /// projected value differs from the previous one. Dispose the result to stop observing.
        /// </summary>
        public IDisposable Observe<TValue>(Func<TS, TValue> selector, Action<TValue> onChanged, IEqualityComparer<TValue>? comparer = null)
        {
            if (selector is null) throw new ArgumentNullException(nameof(selector));
            if (onChanged is null) throw new ArgumentNullException(nameof(onChanged));

            comparer ??= EqualityComparer<TValue>.Default;

            var hasValue = false;
            TValue last = default!;

            void Emit()
            {
                var current = State;
                if (current is null) return;

                var next = selector(current);
                if (hasValue && comparer.Equals(last, next)) return;

                last = next;
                hasValue = true;
                onChanged(next);
            }

            Emit();

            EventHandler handler = (_, _) => Emit();
            StateChanged += handler;
            return new CallbackDisposable(() => StateChanged -= handler);
        }

        #endregion

        #region abstract methods

        public abstract void DisposeState();
        public abstract void CreateState();

        #endregion
    }
}
