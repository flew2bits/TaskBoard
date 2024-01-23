namespace TaskBoard.Users;

public record UserAggregate(Guid Id, string Name, bool IsDeleted)
{
    public static UserAggregate Create(UserCreated evt) => new(evt.UserId, evt.Name, false);
    public UserAggregate Apply(UserDeleted _) => this with { IsDeleted = true };
}
