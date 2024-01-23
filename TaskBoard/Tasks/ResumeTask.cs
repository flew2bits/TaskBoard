using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ResumeTask(Guid TaskAggregateId);

public static class ResumeTaskHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(ResumeTask cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.State != TaskState.OnHold) return (events, CommandResult.OkResult);
        if (task.AssignedTo is null)
            return (events, CommandResult.ErrorResult("Cannot move task to in progress without assignment"));
        events += new TaskResumed(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskResumed(Guid TaskId): IStateChange;