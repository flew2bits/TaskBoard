using Marten;
using Marten.Linq;
using TaskBoard.Users.Views;
using Wolverine.Marten;

namespace TaskBoard.Users;

public record CreateNewUser(string LoginId, string Name);

public static class CreateNewUserHandler
{
    public static async Task<UserDetail?> LoadAsync(CreateNewUser cmd, IDocumentSession session)
        => await session.Query<UserDetail>().SingleOrDefaultAsync(u => u.LoginId == cmd.LoginId);
    
    [AggregateHandler(AggregateType = typeof(UserAggregate))]
    public static (Events, CommandResult) Start(CreateNewUser cmd, UserDetail? user)
    {
        var events = new Events();
        if (user is not null)
            return (events, CommandResult.ErrorResult($"A user with login id {cmd.LoginId} already exists"));
        var id = Guid.NewGuid();
        events += new UserCreated(id, cmd.LoginId, cmd.Name);
        return (events, CommandResult.CreatedResult(id));
    }
}

public record UserCreated(Guid UserId, string LoginId, string Name);