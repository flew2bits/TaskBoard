using Wolverine.Marten;

namespace TaskBoard.Tasks;

public record RespondToConversation(Guid TaskAggregateId, Guid RespondingToNoteId, string Text);

public static class RespondToConversationHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(RespondToConversation cmd, TaskAggregate task)
    {
        if (task.Notes.All(t => t.NoteId != cmd.RespondingToNoteId)) yield break;
        yield return new RespondedToNote(cmd.TaskAggregateId, Guid.NewGuid(), cmd.RespondingToNoteId, cmd.Text);
    }
}

public record RespondedToNote(Guid Id, Guid NoteId, Guid InResponseTo, string Text);