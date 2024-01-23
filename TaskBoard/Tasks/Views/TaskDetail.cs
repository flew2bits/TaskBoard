using Marten;
using Marten.Events.Aggregation;
using TaskBoard.Users;
using TaskBoard.Users.Views;

namespace TaskBoard.Tasks.Views;

public record TaskDetail(Guid Id, string Title, string Priority, TaskState State, string Assignee, Guid? AssigneeId);

public class TaskDetailProjection : SingleStreamProjection<TaskDetail>
{
    public TaskDetailProjection()
    {
        DeleteEvent<TaskDeleted>();
        DeleteEvent<TaskArchived>();
    }
    
    public static TaskDetail Create(TaskStarted evt) => new(evt.TaskId, evt.Title, evt.Priority.ToString(), TaskState.New, "Unassigned", null);

    public async Task<TaskDetail> Apply(TaskAssignedToUser @event, TaskDetail task, IQuerySession session)
    {
        var user = await session.Events.AggregateStreamAsync<UserAggregate>(@event.UserId);
        return task with { Assignee = user!.Name, AssigneeId = user.Id };
    }

    public TaskDetail Apply(TaskUnassigned @event, TaskDetail task) =>
        task with { Assignee = "Unassigned", AssigneeId = null };

    public TaskDetail Apply(PriorityChanged @event, TaskDetail task) =>
        task with { Priority = @event.Priority.ToString() };

    public TaskDetail Apply(WorkStarted @event, TaskDetail task) => task with { State = TaskState.InProgress };

    public TaskDetail Apply(TaskHeld @event, TaskDetail task) => task with { State = TaskState.OnHold };

    public TaskDetail Apply(TaskResumed @event, TaskDetail task) => task with { State = TaskState.InProgress };

    public TaskDetail Apply(TaskRenamed @event, TaskDetail task) => task with { Title = @event.Title };

    public TaskDetail Apply(TaskCompleted _, TaskDetail task) => task with { State = TaskState.Completed };

    public TaskDetail Apply(TaskCanceled _, TaskDetail task) => task with { State = TaskState.Canceled };
}