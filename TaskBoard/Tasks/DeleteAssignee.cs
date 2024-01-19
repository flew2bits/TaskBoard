using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record DeleteAssignee(Guid TaskAggregateId);

public static class DeleteAssigneeHandler {
    [AggregateHandler]
    public static IEnumerable<object> Handle(DeleteAssignee cmd, TaskAggregate task)
    {
        if (task.State is not (TaskState.New or TaskState.InProgress or TaskState.OnHold)) yield break;
        if (task.State is TaskState.InProgress) yield return new TaskHeld(cmd.TaskAggregateId);
        yield return new TaskUnassigned(cmd.TaskAggregateId);
    }
}