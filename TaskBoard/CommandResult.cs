namespace TaskBoard;

public abstract record CommandResult
{
    public sealed record Ok : CommandResult
    {
        private Ok()
        {
        }

        public static Ok Instance => new();
    }

    public sealed record Created(Guid Id) : CommandResult;
    public sealed record Error(string Message) : CommandResult;

    public static Ok OkResult => Ok.Instance;
    public static Created CreatedResult(Guid id) => new Created(id);
    public static Error ErrorResult(string message) => new Error(message);
}