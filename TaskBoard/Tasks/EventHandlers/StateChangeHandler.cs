using Microsoft.AspNetCore.SignalR;
using TaskBoard.Tasks.Hubs;

namespace TaskBoard.Tasks.EventHandlers;

public static class StateChangeHandler
{
    public static async Task Handle(TaskStarted evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.TaskCreated(evt.Id);
    
    public static async Task Handle(WorkStarted evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.Id, "InProgress");
    }

    public static async Task Handle(TaskHeld evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.Id, "OnHold");
    }

    public static async Task Handle(TaskResumed evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.Id, "InProgress");
    }
    
    public static async Task Handle(TaskCompleted evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.Id, "Completed");
    }
        
    public static async Task Handle(TaskCanceled evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.StateChanged(evt.Id, "Canceled");
    }
}