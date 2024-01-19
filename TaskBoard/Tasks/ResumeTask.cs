using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record ResumeTask(Guid TaskAggregateId);

public static class ResumeTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(ResumeTask cmd, TaskAggregate task)
    {
        if (task.State != TaskState.OnHold) yield break;
        if (task.AssignedTo is null)
            throw new InvalidOperationException("Cannot move task to in progress without assignment");
        yield return new TaskResumed(cmd.TaskAggregateId);
    }
}

public record TaskResumed(Guid Id): IStateChange;