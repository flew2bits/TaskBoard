using Microsoft.AspNetCore.SignalR;
using TaskBoard.Tasks.Hubs;

namespace TaskBoard.Tasks.EventHandlers;

public static class TitleChangeHandler
{
    public static async Task Handle(TaskRenamed evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.Renamed(evt.TaskId, evt.Title);
    }
}