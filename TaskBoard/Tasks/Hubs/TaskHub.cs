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

    Task TaskCreated(Guid taskId);

    Task RemoveTask(Guid taskId);

    Task UpdatePriority(Guid taskId, string priority);

    Task Renamed(Guid taskId, string title);
}

public class TaskHub(IMessageBus bus) : Hub<ITaskHub>
{
    private IMessageBus Bus { get; } = bus;

    public async Task RenameTask(string taskIdVal, string title)
    {
        if (!Guid.TryParse(taskIdVal, out var taskId))
        {
            await Clients.Caller.ErrorMessage("The task is is invalid");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            await Clients.Caller.ErrorMessage("The title must have a value");
            return;
        }

        var result = await Bus.InvokeAsync<CommandResult>(new RenameTask(taskId, title));
        if (result is CommandResult.Error error)
            await Clients.Caller.ErrorMessage(error.Message);
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

        var result = await Bus.InvokeAsync<CommandResult>(
            userId == Guid.Empty
                ? new RemoveTaskAssignment(taskId)
                : new AssignTaskToUser(taskId, userId));
       
        if (result is not CommandResult.Error error) return true;
       
        await Clients.Caller.ErrorMessage(error.Message);
        return false;
    }

    public async Task SetPriority(string taskIdVal, string priorityVal)
    {
        if (!Guid.TryParse(taskIdVal, out var taskId))
        {
            await Clients.Caller.ErrorMessage("The task id is invalid");
            return;
        }

        if (!Enum.TryParse<TaskPriority>(priorityVal, out var priority))
        {
            await Clients.Caller.ErrorMessage("The priority is invalid");
            return;
        }

        var result = await Bus.InvokeAsync<CommandResult>(new ChangeTaskPriority(taskId, priority));
        if (result is CommandResult.Error error)
            await Clients.Caller.ErrorMessage(error.Message);
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

        var result = await Bus.InvokeAsync<CommandResult>(command);
        if (result is CommandResult.Error error)
            await Clients.Caller.ErrorMessage(error.Message);
    }
}