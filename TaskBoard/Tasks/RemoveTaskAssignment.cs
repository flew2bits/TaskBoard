using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RemoveTaskAssignment(Guid TaskAggregateId);

public static class RemoveTaskAssignmentHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(RemoveTaskAssignment cmd, TaskAggregate task)
    {
        if (task.AssignedTo is null) yield break;
        if (task.State != TaskState.New && task.State != TaskState.OnHold) throw new InvalidOperationException("Cannot remove assignment");
        yield return new TaskUnassigned(cmd.TaskAggregateId);
    }
}

public record TaskUnassigned(Guid Id);