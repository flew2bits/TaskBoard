using Marten.Events.Aggregation;

namespace TaskBoard.Users.Views;

public record UserDetail(Guid Id, string LoginId, string Name);

public class UserDetailProjection : SingleStreamProjection<UserDetail>
{
    public UserDetailProjection()
    {
        DeleteEvent<UserDeleted>();
    }
    public static UserDetail Create(UserCreated evt) => new UserDetail(evt.UserId, evt.LoginId, evt.Name);
}