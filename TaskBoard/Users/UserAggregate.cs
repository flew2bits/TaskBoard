namespace TaskBoard.Users;

public record UserAggregate(Guid Id, bool IsDeleted)
{
    public static UserAggregate Create(UserCreated evt) => new(evt.Id, false);
    public UserAggregate Apply(UserDeleted _) => this with { IsDeleted = true };
}
