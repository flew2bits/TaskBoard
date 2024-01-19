using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ArchiveTask(Guid TaskAggregateId);

public static class ArchiveTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(ArchiveTask cmd, TaskAggregate task)
    {
        if (task.State is not (TaskState.Canceled or TaskState.Completed)) yield break;
        yield return new TaskArchived(cmd.TaskAggregateId);
    }
}

public record TaskArchived(Guid Id);