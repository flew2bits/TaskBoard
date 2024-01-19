using Marten;
using TaskBoard.Tasks;
using TaskBoard.Tasks.Views;
using Wolverine;
using Wolverine.Marten;

namespace TaskBoard.Users;

public record DeleteUser(Guid UserAggregateId);

public static class DeleteUserHandler
{
    public static async Task<IEnumerable<TaskDetail>> LoadAsync(DeleteUser cmd, IDocumentSession session)
    {
        var tasks = await session.Query<TaskDetail>().Where(t => t.AssigneeId == cmd.UserAggregateId).ToListAsync();
        return tasks.Where(t => t.State is TaskState.New or TaskState.InProgress or TaskState.OnHold);
    }
    
    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(DeleteUser cmd, UserAggregate user, IEnumerable<TaskDetail> tasks)
    {
        var events = new Events();
        var messages = new OutgoingMessages();
        if (user.IsDeleted) return (events, messages);

        var orderedUri = new Uri("local://ordered");
        
        events += new UserDeleted(cmd.UserAggregateId);

        foreach (var task in tasks)
        {
            if (task.State == TaskState.InProgress)
                messages.Add(new PlaceTaskOnHold(task.Id).ToDestination(orderedUri));
            messages.Add(new RemoveTaskAssignment(task.Id).ToDestination(orderedUri));
        }

        return (events, messages);
    }
}

public record UserDeleted(Guid Id);