using Marten.Events.Aggregation;

namespace TaskBoard.Users.Views;

public record UserDetail(Guid Id, string UserId, string Name);

public class UserDetailProjection : SingleStreamProjection<UserDetail>
{
    public static UserDetail Create(UserCreated evt) => new UserDetail(evt.Id, evt.UserId, evt.Name);
}