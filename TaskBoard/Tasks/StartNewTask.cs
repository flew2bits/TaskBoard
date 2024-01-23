using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartNewTask(string Title, TaskPriority Priority);

public static class StartNewTaskHandler
{
    [AggregateHandler(AggregateType = typeof(TaskAggregate))]
    public static (Events, CommandResult) Handle(StartNewTask cmd)
    {
        var events = new Events();
        var id = Guid.NewGuid();
        events += new TaskStarted(id, cmd.Title, cmd.Priority);
        return (events, CommandResult.CreatedResult(id));
    }
}

public record TaskStarted(Guid TaskId, string Title, TaskPriority Priority);