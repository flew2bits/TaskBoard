using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record StartConversation(Guid TaskAggregateId, string Note);

public static class StartConversationHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(StartConversation cmd, TaskAggregate task)
    {
        var events = new Events();
        var noteId = Guid.NewGuid();
        events += new ConversationStarted(cmd.TaskAggregateId, noteId, cmd.Note);
        return (events, CommandResult.CreatedResult(noteId));
    }
        
}

public record ConversationStarted(Guid TaskId, Guid NoteId, string Note);