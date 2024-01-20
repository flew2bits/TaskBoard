using Microsoft.AspNetCore.SignalR;
using TaskBoard.Tasks.Hubs;

namespace TaskBoard.Tasks.EventHandlers;

public static class StateChangeHandler
{
    public static async Task Handle(TaskStarted evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.TaskCreated(evt.TaskId);
    
    public static async Task Handle(WorkStarted evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.TaskId, "InProgress");
    }

    public static async Task Handle(TaskHeld evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.TaskId, "OnHold");
    }

    public static async Task Handle(TaskResumed evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.TaskId, "InProgress");
    }
    
    public static async Task Handle(TaskCompleted evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.TaskId, "Completed");
    }
        
    public static async Task Handle(TaskCanceled evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.StateChanged(evt.TaskId, "Canceled");

    public static async Task Handle(TaskDeleted evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.RemoveTask(evt.TaskId);

    public static async Task Handle(TaskArchived evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.RemoveTask(evt.TaskId);
}