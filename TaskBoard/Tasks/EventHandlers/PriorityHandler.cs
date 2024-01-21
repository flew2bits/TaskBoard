using Microsoft.AspNetCore.SignalR;
using TaskBoard.Tasks.Hubs;

namespace TaskBoard.Tasks.EventHandlers;

public static class PriorityHandler
{
    public static async Task Handle(PriorityChanged evt, IHubContext<TaskHub, ITaskHub> hub) =>
        await hub.Clients.All.UpdatePriority(evt.TaskId, evt.Priority.ToString());
}