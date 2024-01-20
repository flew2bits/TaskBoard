using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using TaskBoard.Tasks;

namespace TaskBoard.Users.Views;

public record UserActiveTasks(Guid Id, string Name, Guid[] InProgress, Guid[] OnHold);

file class TaskUnassignedGrouper : IAggregateGrouper<Guid>
{
    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        try
        {

            var unassignedEvents = events.OfType<IEvent<TaskUnassigned>>().ToArray();

            if (unassignedEvents.Length == 0) return;

            var taskIds = unassignedEvents.Select(e => e.Data.TaskId).ToList();

            var taskAssignedEvents = await session.Events.QueryRawEventDataOnly<TaskAssignedToUser>()
                .Where(t => taskIds.Contains(t.TaskId))
                .ToListAsync().ConfigureAwait(false);

            var mapping = taskAssignedEvents.GroupBy(t => t.TaskId).Select(t => t.Last())
                .ToDictionary(t => t.TaskId, t => t.UserId);

            grouping.AddEvents<TaskUnassigned>(e => mapping[e.TaskId], unassignedEvents);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

file class TaskStateChangedGrouper : IAggregateGrouper<Guid>
{
    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        try
        {
            events = events as IEvent[] ?? events.ToArray();

            var stateChangeEvents = events.OfType<IEvent<IStateChange>>().ToArray();

            if (stateChangeEvents.Length == 0) return;

            var taskIds = stateChangeEvents.Select(e => e.Data.TaskId).Distinct().ToList();

            var taskAssignedEvents = (await session.Events.QueryRawEventDataOnly<TaskAssignedToUser>().ToListAsync())
                .Where(t => taskIds.Contains(t.TaskId))
                .GroupBy(e => e.TaskId)
                .Select(g => (g.Key, g.Last().UserId))
                .ToDictionary(g => g.Key, v => v.UserId);

            grouping.AddEvents<IStateChange>(e => taskAssignedEvents[e.TaskId], stateChangeEvents);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

file class TaskAssignedToUserGrouper: IAggregateGrouper<Guid>
{
    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        try
        {
            var assignedEvents = events.OfType<IEvent<TaskAssignedToUser>>().ToArray();

            if (assignedEvents.Length == 0) return;

            var taskIds = assignedEvents.Select(e => e.Data.TaskId).ToList();

            var taskAssignedEvents = (await session.Events.QueryRawEventDataOnly<TaskAssignedToUser>()
                .ToListAsync()).Where(t => taskIds.Contains(t.TaskId));

            var mapping = taskAssignedEvents.GroupBy(t => t.TaskId).Select(g => g.Reverse().Take(2))
                .ToDictionary(t => t.First().TaskId, t => t.Select(u => u.UserId));

            grouping.AddEvents<TaskAssignedToUser>(e => mapping[e.TaskId], assignedEvents);
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
        CustomGrouping(new TaskStateChangedGrouper());
        CustomGrouping(new TaskUnassignedGrouper());

        DeleteEvent<UserDeleted>();
    }

    public static UserActiveTasks Create(UserCreated evt) =>
        new(evt.UserId, evt.Name, Array.Empty<Guid>(), Array.Empty<Guid>());

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
        {            Console.WriteLine(ex.Message);

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
        {            Console.WriteLine(ex.Message);

            throw;
        }
    }
}