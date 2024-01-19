using Wolverine.Marten;

namespace TaskBoard.Users;

public record DeleteUser(Guid UserAggregateId);

public static class DeleteUserHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(DeleteUser cmd, UserAggregate user)
    {
        if (user.IsDeleted) yield break;
        yield return new UserDeleted(cmd.UserAggregateId);
    }
}

public record UserDeleted(Guid Id);