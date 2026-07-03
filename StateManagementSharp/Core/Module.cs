namespace StateManagementSharp.Core
{
    public interface IModule
    {
        void DisposeState();
        void CreateState();
    }
}
