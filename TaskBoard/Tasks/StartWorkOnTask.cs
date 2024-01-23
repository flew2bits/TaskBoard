using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartWorkOnTask(Guid TaskAggregateId);

public static class StartWorkOnTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(StartWorkOnTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State != TaskState.New) return (events, CommandResult.OkResult);
        if (task.AssignedTo is null) return (events, CommandResult.ErrorResult("Can't start work without assignment"));
        events += new WorkStarted(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record WorkStarted(Guid TaskId): IStateChange;