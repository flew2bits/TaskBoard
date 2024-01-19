using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ChangeTaskPriority(Guid TaskAggregateId, TaskPriority Priority);

public static class ChangeTaskPriorityHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(ChangeTaskPriority cmd, TaskAggregate task)
    {
        if (task.Priority == cmd.Priority) yield break;
        if (task.State is TaskState.Completed or TaskState.Canceled or TaskState.Archived) yield break;
        yield return new PriorityChanged(cmd.TaskAggregateId, cmd.Priority);
    }
}

public record PriorityChanged(Guid Id, TaskPriority Priority);