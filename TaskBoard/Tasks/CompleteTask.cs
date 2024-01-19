using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record CompleteTask(Guid TaskAggregateId);

public static class CompleteTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(CompleteTask cmd, TaskAggregate task)
    {
        if (task.State is not TaskState.InProgress) yield break;
        yield return new TaskCompleted(cmd.TaskAggregateId);
    }
}

public record TaskCompleted(Guid Id): IStateChange;