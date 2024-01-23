using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record CancelTask(Guid TaskAggregateId);

public static class CancelTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(CancelTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State is TaskState.InProgress or TaskState.OnHold) 
         events += new TaskCanceled(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskCanceled(Guid TaskId): IStateChange;