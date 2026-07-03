using System;
using System.Collections.Generic;
using Microsoft.Maui.Dispatching;
using StateManagementSharp;

namespace StateManagementSharp.Maui
{
    /// <summary>
    /// Base class for view models that project store state into bindable values.
    /// Each <see cref="Bind{TS,TR,TValue}(ModuleBase{TS,TR},System.Func{TS,TValue},System.Collections.Generic.IEqualityComparer{TValue})"/>
    /// call subscribes to a selector and pushes changes onto the UI thread via <see cref="IDispatcher"/>.
    /// Dispose the view model to release all subscriptions.
    /// </summary>
    public abstract class StoreBindableObject : IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        protected StoreBindableObject(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>Binds a projection of a module's state to a UI-thread-marshaled <see cref="BindableValue{TValue}"/>.</summary>
        protected BindableValue<TValue> Bind<TS, TR, TValue>(
            ModuleBase<TS, TR> module,
            Func<TS, TValue> selector,
            IEqualityComparer<TValue>? comparer = null)
            where TS : IState
            where TR : IRootState
        {
            if (module is null) throw new ArgumentNullException(nameof(module));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            var holder = new BindableValue<TValue>();
            _subscriptions.Add(module.Observe(selector, value => Apply(holder, value), comparer));
            return holder;
        }

        /// <summary>Binds a projection of the root state to a UI-thread-marshaled <see cref="BindableValue{TValue}"/>.</summary>
        protected BindableValue<TValue> Bind<TR, TValue>(
            Store<TR> store,
            Func<TR, TValue> selector,
            IEqualityComparer<TValue>? comparer = null)
            where TR : IRootState
        {
            if (store is null) throw new ArgumentNullException(nameof(store));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            var holder = new BindableValue<TValue>();
            _subscriptions.Add(store.Observe(selector, value => Apply(holder, value), comparer));
            return holder;
        }

        private void Apply<TValue>(BindableValue<TValue> holder, TValue value)
        {
            if (_dispatcher.IsDispatchRequired)
                _dispatcher.Dispatch(() => holder.Value = value);
            else
                holder.Value = value;
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();
        }
    }
}
