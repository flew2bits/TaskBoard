using Marten;
using TaskBoard.Tasks;
using TaskBoard.Tasks.Views;
using Wolverine;

namespace TaskBoard.Users.Workflow;

public static class UserDeletedXHandler
{
    public static async Task Handle(UserDeleted evt, IDocumentSession session, IMessageBus bus)
    {
        var endpoint = bus.EndpointFor("ordered");
        
        var interestingStates = new List<TaskState> { TaskState.New, TaskState.InProgress, TaskState.OnHold };

        var tasks = session.Query<TaskDetail>().ToList()
            .Where(td => td.AssigneeId == evt.Id && interestingStates.Contains(td.State)).ToArray();

        foreach (var task in tasks)
        {
            if (task.State == TaskState.InProgress) await endpoint.SendAsync(new PlaceTaskOnHold(task.Id));
            await endpoint.SendAsync(new RemoveTaskAssignment(task.Id));
        }
    }
}