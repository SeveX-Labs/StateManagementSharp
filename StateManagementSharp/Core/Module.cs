using System;

namespace StateManagementSharp.Core
{
    public interface IModule
    {
        /// <summary>Raised after this module commits a mutation and replaces its state.</summary>
        event EventHandler? StateChanged;

        void DisposeState();
        void CreateState();
    }
}
