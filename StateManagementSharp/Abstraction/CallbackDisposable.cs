using System;

namespace StateManagementSharp
{
    /// <summary>
    /// A lightweight <see cref="IDisposable"/> that runs a callback once on disposal.
    /// Used to unsubscribe reactive observers created by <c>Observe</c>. Double-dispose is a no-op.
    /// </summary>
    internal sealed class CallbackDisposable : IDisposable
    {
        private Action? _onDispose;

        public CallbackDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            var callback = _onDispose;
            _onDispose = null;
            callback?.Invoke();
        }
    }
}
