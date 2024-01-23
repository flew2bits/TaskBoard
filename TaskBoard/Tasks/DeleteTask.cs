using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record DeleteTask(Guid TaskAggregateId);

public static class DeleteTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(DeleteTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State != TaskState.New)
            return (events, CommandResult.ErrorResult("A task can only be deleted from the new state"));
        events += new TaskDeleted(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskDeleted(Guid TaskId);