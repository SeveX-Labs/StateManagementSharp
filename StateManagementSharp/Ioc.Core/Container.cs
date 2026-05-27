namespace StateManagementSharp.Ioc.Core
{
    public interface Container
    {
        void Build();
        T Resolve<T>() where T : notnull;

    }
}
