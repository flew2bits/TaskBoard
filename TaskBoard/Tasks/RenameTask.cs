using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RenameTask(Guid TaskAggregateId, string Title);

public static class RenameTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(RenameTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.Title == cmd.Title) return (events, CommandResult.OkResult);
        if (task.State is not (TaskState.New or TaskState.InProgress or TaskState.OnHold))
            return (events, CommandResult.ErrorResult("Cannot rename task in current state"));
        events += new TaskRenamed(cmd.TaskAggregateId, cmd.Title);
        return (events, CommandResult.OkResult);
    }
}

public record TaskRenamed(Guid TaskId, string Title);