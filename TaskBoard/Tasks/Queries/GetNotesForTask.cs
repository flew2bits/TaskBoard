using TaskBoard.Tasks.Views;
using Wolverine.Http;
using Wolverine.Http.Marten;

namespace TaskBoard.Tasks.Queries;

public class GetNotesForTask
{
    [WolverineGet("/api/{id}/notes")]
    public static TaskNotes GetNotes([Document] TaskNotes notes) => notes;
}