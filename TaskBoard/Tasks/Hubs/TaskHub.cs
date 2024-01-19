using Marten;
using Microsoft.AspNetCore.SignalR;
using Wolverine;

namespace TaskBoard.Tasks.Hubs;

file static class ActionNames
{
    public const string Start = nameof(Start);
    public const string Delete = nameof(Delete);
    public const string Pause = nameof(Pause);
    public const string Complete = nameof(Complete);
    public const string Cancel = nameof(Cancel);
    public const string Resume = nameof(Resume);
    public const string Archive = nameof(Archive);
}

public interface ITaskHub
{
    Task StateChanged(Guid taskId, string state);
    Task ErrorMessage(string message);

    Task Unassigned(Guid taskId);
    Task Reassigned(Guid taskId, Guid userId);
}

public class TaskHub : Hub<ITaskHub>
{
    private readonly IMessageBus _bus;

    public TaskHub(IMessageBus bus)
    {
        _bus = bus;
    }

    public async Task<bool> ReassignTask(string taskIdVal, string userIdVal)
    {
        if (!Guid.TryParse(taskIdVal, out var taskId))
        {
            await Clients.Caller.ErrorMessage("The task id is invalid");
            return false;
        }

        if (!Guid.TryParse(userIdVal, out var userId))
        {
            await Clients.Caller.ErrorMessage("The user id is invalid");
            return false;
        }

        try
        {
            await _bus.InvokeAsync(
                userId == Guid.Empty
                    ? new RemoveTaskAssignment(taskId)
                    : new AssignTaskToUser(taskId, userId));
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.ErrorMessage(ex.Message);
            return false;
        }

        return true;
    }

    public async Task ChangeTaskState(string action, string taskIdVal)
    {
        if (!Guid.TryParse(taskIdVal, out var taskId))
        {
            await Clients.Caller.ErrorMessage("The task id is invalid");
            return;
        }

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

        if (command is null)
        {
            await Clients.Caller.ErrorMessage("The action is invalid");
            return;
        }

        try
        {
            await _bus.InvokeAsync(command);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.ErrorMessage(ex.Message);
        }
    }
}