namespace TaskBoard.Tasks;

public interface IStateChange
{
    Guid TaskId { get; }
}