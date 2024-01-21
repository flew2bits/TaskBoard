using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using TaskBoard.Tasks;

namespace TaskBoard.Users.Views;

public record UserActiveTasks(Guid Id, string Name, Guid[] InProgress, Guid[] OnHold);

file class TaskAssignedToUserGrouper : IAggregateGrouper<Guid>
{
    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        try
        {
            var assignedEvents = events.OfType<IEvent<TaskAssignedToUser>>().ToArray();

            if (assignedEvents.Length == 0) return;

            var taskIds = assignedEvents.Select(e => e.Data.TaskId).ToList();

            var taskAssignedEvents = (await session.Events.QueryAllRawEvents()
                    .Where(e => e.EventTypeName == EventMappingExtensions.GetEventTypeName<TaskAssignedToUser>())
                    .ToListAsync())
                .Cast<IEvent<TaskAssignedToUser>>()
                .Where(t => taskIds.Contains(t.Data.TaskId))
                .GroupBy(t => t.Data.TaskId)
                .ToDictionary(t => t.Key);

            foreach (var evt in assignedEvents)
            {
                var previousId = taskAssignedEvents[evt.Data.TaskId].TakeWhile(e => e.Id != evt.Id).LastOrDefault()?.Data.UserId;
                if (previousId is not null && previousId != evt.Data.UserId)
                    grouping.AddEvent(previousId.Value, evt);
                grouping.AddEvent(evt.Data.UserId, evt);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

public class UserActiveTasksProjection : MultiStreamProjection<UserActiveTasks, Guid>
{
    public UserActiveTasksProjection()
    {
        Identity<UserCreated>(u => u.UserId);
        CustomGrouping(new TaskAssignedToUserGrouper());

        CustomGrouping(
            new MostRecentEventIdentityGrouper<IStateChange, TaskAssignedToUser>(i => i.TaskId, a => a.TaskId,
                a => a.UserId));
        CustomGrouping(
            new MostRecentEventIdentityGrouper<TaskUnassigned, TaskAssignedToUser>(u => u.TaskId, a => a.TaskId,
                a => a.UserId));

        DeleteEvent<UserDeleted>();
    }

    public static UserActiveTasks Create(UserCreated evt) => new(evt.UserId, evt.Name, Array.Empty<Guid>(), Array.Empty<Guid>());

    public UserActiveTasks Apply(TaskUnassigned evt, UserActiveTasks view) => RemoveInternal(evt.TaskId, view);

    private static UserActiveTasks RemoveInternal(Guid id, UserActiveTasks view)
    {
        try
        {
            var toRemove = new[] { id };
            return view with
            {
                InProgress = view.InProgress.Except(toRemove).ToArray(), OnHold = view.OnHold.Except(toRemove).ToArray()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<UserActiveTasks> Apply(TaskAssignedToUser evt, UserActiveTasks view, IQuerySession session)
    {
        view = RemoveInternal(evt.TaskId, view);

        try
        {
            if (evt.UserId != view.Id)
                return view;

            var task = await session.Events.AggregateStreamAsync<TaskAggregate>(evt.TaskId);
            if (task is null) return view;
            return task.State switch
            {
                TaskState.InProgress => view with { InProgress = view.InProgress.Append(evt.TaskId).ToArray() },
                TaskState.OnHold => view with { OnHold = view.OnHold.Append(evt.TaskId).ToArray() },
                _ => view
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            throw;
        }
    }

    public UserActiveTasks Apply(IStateChange evt, UserActiveTasks view)
    {
        try
        {
            view = RemoveInternal(evt.TaskId, view);
            return evt switch
            {
                WorkStarted or TaskResumed => view with { InProgress = view.InProgress.Append(evt.TaskId).ToArray() },
                TaskHeld => view with { OnHold = view.OnHold.Append(evt.TaskId).ToArray() },
                _ => view
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            throw;
        }
    }
}