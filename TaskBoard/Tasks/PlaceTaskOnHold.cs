using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record PlaceTaskOnHold(Guid TaskAggregateId);

public static class PlaceTaskOnHoldHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(PlaceTaskOnHold cmd, TaskAggregate task)
    {
        if (task.State != TaskState.InProgress) yield break;
        yield return new TaskHeld(cmd.TaskAggregateId);
    }
}

public record TaskHeld(Guid TaskId): IStateChange;