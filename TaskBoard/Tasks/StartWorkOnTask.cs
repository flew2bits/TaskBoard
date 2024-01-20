using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartWorkOnTask(Guid TaskAggregateId);

public static class StartWorkOnTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(StartWorkOnTask cmd, TaskAggregate task)
    {
        if (task.State != TaskState.New) yield break;
        if (task.AssignedTo is null) throw new InvalidOperationException("Can't start work without assignment");
        yield return new WorkStarted(cmd.TaskAggregateId);
    }
}

public record WorkStarted(Guid TaskId): IStateChange;