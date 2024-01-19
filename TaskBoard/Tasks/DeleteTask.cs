using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record DeleteTask(Guid TaskAggregateId);

public static class DeleteTaskHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(DeleteTask cmd, TaskAggregate task)
    {
        if (task.State != TaskState.New)
            throw new InvalidOperationException("A task can only be deleted from the new state");
        yield return new TaskDeleted(cmd.TaskAggregateId);
    }
}

public record TaskDeleted(Guid Id);