using Marten;
using TaskBoard.Users.Views;
using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record AssignTaskToUser(Guid TaskAggregateId, Guid UserId);

public static class AssignTaskToUserHandler
{
    public static Task<UserDetail?> LoadAsync(AssignTaskToUser cmd, IDocumentSession session)
        => session.LoadAsync<UserDetail>(cmd.UserId);

    [AggregateHandler]
    public static IEnumerable<object> Handle(AssignTaskToUser cmd, TaskAggregate task, UserDetail? user)
    {
        if (user is null) throw new InvalidOperationException("User does not exist");
        if (task.AssignedTo == user.Id) yield break;
        if (task.State is not (TaskState.New or TaskState.InProgress or TaskState.OnHold)) yield break;
        yield return new TaskAssignedToUser(cmd.TaskAggregateId, cmd.UserId);
    }
}

public record TaskAssignedToUser(Guid TaskId, Guid UserId);