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
    public static (Events, CommandResult) Handle(AssignTaskToUser cmd, TaskAggregate task, UserDetail? user)
    {
        var events = new Events();
        if (user is null) return (events, CommandResult.ErrorResult("User does not exists"));
        if (task.AssignedTo == user.Id) return (events, CommandResult.OkResult);
        if (task.State is not (TaskState.New or TaskState.InProgress or TaskState.OnHold))
            return(events, CommandResult.ErrorResult("Assignment cannot be changed in current state"));
        events += new TaskAssignedToUser(cmd.TaskAggregateId, cmd.UserId);
        return (events, CommandResult.OkResult);
    }
}

public record TaskAssignedToUser(Guid TaskId, Guid UserId);