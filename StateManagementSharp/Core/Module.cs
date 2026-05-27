namespace StateManagementSharp.Core
{
    public interface Module
    {
        void DisposeState();
        void CreateState();
    }
}
