using PropertyChanged;

namespace StateManagementSharp.Maui
{
    /// <summary>
    /// A single observable value woven with <c>PropertyChanged.Fody</c>, so assigning
    /// <see cref="Value"/> raises <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>.
    /// Bind XAML to the <c>.Value</c> path (for example <c>{Binding FirstName.Value}</c>).
    /// The value is written by <see cref="StoreBindableObject"/>; from a view it is read-only.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public sealed class BindableValue<T>
    {
        internal BindableValue()
        {
        }

        public T Value { get; internal set; } = default!;
    }
}
