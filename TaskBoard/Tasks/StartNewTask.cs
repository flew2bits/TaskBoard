using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartNewTask(Guid TaskAggregateId, string Title, TaskPriority Priority);

public static class StartNewTaskHandler
{
    [AggregateHandler(AggregateType = typeof(TaskAggregate))]
    public static IEnumerable<object> Handle(StartNewTask cmd)
    {
        yield return new TaskStarted(cmd.TaskAggregateId, cmd.Title, cmd.Priority);
    }
}

public record TaskStarted(Guid Id, string Title, TaskPriority Priority);