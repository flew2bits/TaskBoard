using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartConversation(Guid TaskAggregateId, string Note);

public static class StartConversationHandler
{
    [AggregateHandler]
    public static ConversationStarted Handle(StartConversation cmd, TaskAggregate task) =>
        new(cmd.TaskAggregateId, Guid.NewGuid(), cmd.Note);
}

public record ConversationStarted(Guid TaskId, Guid NoteId, string Note);