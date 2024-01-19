using Marten.Events.Aggregation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskBoard.Tasks.Views;

public record TaskNotes(Guid Id, SingleNote[] Notes);

public record SingleNote(Guid NoteId, string Text, SingleNote[] Responses);

public class TaskNotesProjection : SingleStreamProjection<TaskNotes>
{
    public static TaskNotes Create(TaskStarted evt) => new TaskNotes(evt.Id, Array.Empty<SingleNote>());

    public TaskNotes Apply(ConversationStarted evt, TaskNotes notes) =>
        notes with
        {
            Notes = notes.Notes.Append(new SingleNote(evt.NoteId, evt.Note, Array.Empty<SingleNote>())).ToArray()
        };

    public TaskNotes Apply(RespondedToNote evt, TaskNotes notes) =>
        notes with
        {
            Notes = AttachNoteToParent(notes.Notes, evt.InResponseTo, 
                new SingleNote(evt.NoteId, evt.Text, Array.Empty<SingleNote>()))
        };

    private static SingleNote[] AttachNoteToParent(SingleNote[] notes, Guid parentId, SingleNote child) =>
        notes.Select(n => n with { Responses = 
                n.NoteId == parentId 
                    ? n.Responses.Append(child).ToArray()
                    : AttachNoteToParent(n.Responses, parentId, child) })
            .ToArray();
}