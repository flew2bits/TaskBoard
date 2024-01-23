using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskBoard.Tasks;
using TaskBoard.Tasks.Views;
using TaskBoard.Users;
using TaskBoard.Users.Views;
using Wolverine;

namespace TaskBoard.Pages;

public static class ActionNames
{
    public const string Start = nameof(Start);
    public const string Delete = nameof(Delete);
    public const string Pause = nameof(Pause);
    public const string Complete = nameof(Complete);
    public const string Cancel = nameof(Cancel);
    public const string Resume = nameof(Resume);
    public const string Archive = nameof(Archive);
}

public class Index : PageModel
{
    public static string MessageIdForTask(Guid taskId) => $"{taskId}_Message";

    private readonly IMessageBus _bus;

    public Index(IMessageBus bus)
    {
        _bus = bus;
    }

    public UserDetail[] Users { get; set; } = Array.Empty<UserDetail>();
    public TaskDetail[] Tasks { get; set; } = Array.Empty<TaskDetail>();

    public void OnGet([FromServices] IQuerySession session)
    {
        Users = session.Query<UserDetail>().ToArray();
        Tasks = session.Query<TaskDetail>().ToArray();
    }

    public async Task<IActionResult> OnPostAddUser(string userid, string name)
    {
        var result = await _bus.InvokeAsync<CommandResult>(new CreateNewUser(userid, name));
        if (result is CommandResult.Error error)
            TempData["Message"] = error.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateTask(string title, TaskPriority priority)
    {
        if (!Enum.IsDefined(priority)) return BadRequest("Invalid priority");
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("You must provide a valid title");
        await _bus.InvokeAsync(new StartNewTask(title, priority));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignTask(Guid taskId, Guid taskAssignee)
    {
        try
        {
            await _bus.InvokeAsync(taskAssignee == Guid.Empty
                ? new RemoveTaskAssignment(taskId)
                : new AssignTaskToUser(taskId, taskAssignee));
        }
        catch (InvalidOperationException ex)
        {
            TempData[MessageIdForTask(taskId)] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePriority(Guid taskId, TaskPriority priority)
    {
        await _bus.InvokeAsync(new ChangeTaskPriority(taskId, priority));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeState(Guid taskId, string action)
    {
        object? command = action switch
        {
            ActionNames.Start => new StartWorkOnTask(taskId),
            ActionNames.Delete => new DeleteTask(taskId),
            ActionNames.Pause => new PlaceTaskOnHold(taskId),
            ActionNames.Resume => new ResumeTask(taskId),
            ActionNames.Complete => new CompleteTask(taskId),
            ActionNames.Cancel => new CancelTask(taskId),
            ActionNames.Archive => new ArchiveTask(taskId),
            _ => null
        };

        if (command is null) return BadRequest("Unrecognized command");

        try
        {
            await _bus.InvokeAsync(command);
        }
        catch (InvalidOperationException ex)
        {
            TempData[MessageIdForTask(taskId)] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRenameTask(Guid taskId, string title)
    {
        await _bus.InvokeAsync(new RenameTask(taskId, title));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddNote(Guid taskId, Guid inResponseTo, string note)
    {
        if (string.IsNullOrWhiteSpace(note)) return RedirectToPage();

        await _bus.InvokeAsync(inResponseTo == Guid.Empty
            ? new StartConversation(taskId, note)
            : new RespondToConversation(taskId, inResponseTo, note));

        return RedirectToPage();
    }
}