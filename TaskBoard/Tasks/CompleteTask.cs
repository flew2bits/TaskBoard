using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record CompleteTask(Guid TaskAggregateId);

public static class CompleteTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(CompleteTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State is not TaskState.InProgress) 
            events += new TaskCompleted(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskCompleted(Guid TaskId): IStateChange;