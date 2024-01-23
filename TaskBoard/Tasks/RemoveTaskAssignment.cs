using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RemoveTaskAssignment(Guid TaskAggregateId);

public static class RemoveTaskAssignmentHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(RemoveTaskAssignment cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.AssignedTo is null) return (events, CommandResult.OkResult);
        if (task.State is not (TaskState.New or TaskState.OnHold))
            return (events, CommandResult.ErrorResult("Cannot remove assignment"));
        events += new TaskUnassigned(cmd.TaskAggregateId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskUnassigned(Guid TaskId);