using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record CancelTask(Guid TaskAggregateId);

public static class CancelTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(CancelTask cmd, TaskAggregate task)
    {
        if (task.State is not (TaskState.InProgress or TaskState.OnHold)) yield break;
        yield return new TaskCanceled(cmd.TaskAggregateId);
    }
}

public record TaskCanceled(Guid Id): IStateChange;