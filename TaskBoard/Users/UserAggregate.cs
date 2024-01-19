namespace TaskBoard.Users;

public record UserAggregate(Guid Id)
{
    public static UserAggregate Create(UserCreated evt) => new(evt.Id);
}
