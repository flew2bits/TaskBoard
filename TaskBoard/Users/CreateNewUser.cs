using Marten;
using TaskBoard.Users.Views;
using Wolverine.Marten;

namespace TaskBoard.Users;

public record CreateNewUser(Guid UserAggregateId, string UserId, string Name);

public static class CreateNewUserHandler
{
    public static async Task LoadAsync(CreateNewUser cmd, IDocumentSession session)
    {
        if (await session.Query<UserDetail>().AnyAsync(u => u.LoginId == cmd.UserId))
            throw new InvalidOperationException("User already exists");
    }
    
    [AggregateHandler(AggregateType = typeof(UserAggregate))]
    public static IEnumerable<object> Start(CreateNewUser cmd)
    {
        yield return new UserCreated(cmd.UserAggregateId, cmd.UserId, cmd.Name);
    }
}

public record UserCreated(Guid UserId, string LoginId, string Name);