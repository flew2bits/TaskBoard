using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RespondToConversation(Guid TaskAggregateId, Guid RespondingToNoteId, string Text);

public static class RespondToConversationHandler
{
    [AggregateHandler]
    public static (Events, CommandResult) Handle(RespondToConversation cmd, TaskAggregate task)
    {
        var events = new Events();
        if (task.Notes.All(t => t.NoteId != cmd.RespondingToNoteId)) return (events, CommandResult.ErrorResult("Could not find note"));
        events += new RespondedToNote(cmd.TaskAggregateId, Guid.NewGuid(), cmd.RespondingToNoteId, cmd.Text);
        return (events, CommandResult.OkResult);
    }
}

public record RespondedToNote(Guid TaskId, Guid NoteId, Guid InResponseTo, string Text);