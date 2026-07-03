using System.Threading.Tasks;

namespace StateManagementSharp
{
    public interface IAction
    {
    }

    public interface IAction<TS, TR> : IAction where TS : IState where TR : IRootState
    {
        Task Execute(ActionContext<TS, TR> context, object? payload);
    }

}
