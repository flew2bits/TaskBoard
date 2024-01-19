using Microsoft.AspNetCore.SignalR;
using TaskBoard.Tasks.Hubs;

namespace TaskBoard.Tasks.EventHandlers;

public static class AssignmentChangeHandler
{
    public static async Task Handle(TaskUnassigned evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.Unassigned(evt.Id);
    }

    public static async Task Handle(TaskAssignedToUser evt, IHubContext<TaskHub, ITaskHub> hub)
    {
        await hub.Clients.All.Reassigned(evt.Id, evt.UserId);
    }
}