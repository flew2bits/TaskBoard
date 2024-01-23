using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ChangeTaskPriority(Guid TaskAggregateId, TaskPriority Priority);

public static class ChangeTaskPriorityHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(ChangeTaskPriority cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.Priority == cmd.Priority) return (events, CommandResult.OkResult);
        if (task.State is TaskState.Completed or TaskState.Canceled or TaskState.Archived)
            return (events, CommandResult.ErrorResult("Can not change priority in current state"));
        events +=  new PriorityChanged(cmd.TaskAggregateId, cmd.Priority);
        return (events, CommandResult.OkResult);
    }
}

public record PriorityChanged(Guid TaskId, TaskPriority Priority);