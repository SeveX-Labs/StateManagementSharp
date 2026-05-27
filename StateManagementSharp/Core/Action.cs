using System.Threading.Tasks;

namespace StateManagementSharp
{
    public interface Action
    {
    }

    public interface Action<TS, TR> : Action where TS : State where TR : RootState
    {
        Task Execute(ActionContext<TS, TR> context, object? payload);
    }

}
