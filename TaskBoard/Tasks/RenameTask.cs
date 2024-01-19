using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RenameTask(Guid TaskAggregateId, string Title);

public static class RenameTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(RenameTask cmd, TaskAggregate task)
    {
        if (task.Title == cmd.Title) yield break;
        if (task.State is not (TaskState.New or TaskState.InProgress or TaskState.OnHold)) yield break;
        yield return new TaskRenamed(cmd.TaskAggregateId, cmd.Title);
    }
}

public record TaskRenamed(Guid TaskId, string Title);