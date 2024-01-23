using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record PlaceTaskOnHold(Guid TaskAggregateId);

public static class PlaceTaskOnHoldHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(PlaceTaskOnHold cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State is TaskState.InProgress)
            events += new TaskHeld(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskHeld(Guid TaskId): IStateChange;