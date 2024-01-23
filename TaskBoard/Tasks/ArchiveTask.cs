using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ArchiveTask(Guid TaskAggregateId);

public static class ArchiveTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(ArchiveTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State is TaskState.Canceled or TaskState.Completed)
            events += new TaskArchived(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskArchived(Guid TaskId);