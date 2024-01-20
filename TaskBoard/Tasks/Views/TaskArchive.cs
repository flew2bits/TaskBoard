using Marten;
using Marten.Events.Aggregation;
using TaskBoard.Users.Views;

namespace TaskBoard.Tasks.Views;

public record TaskArchive(Guid Id, string Title, TaskPriority Priority, string[] Assignees);

public class TaskArchiveProjection: SingleStreamProjection<TaskArchive>
{
    public static async Task<TaskArchive> Create(TaskArchived evt, IQuerySession session)
    {
        var task = await session.Events.AggregateStreamAsync<TaskAggregate>(evt.TaskId);
        if (task is null) throw new InvalidOperationException("Could not load task aggregate");
        var assigneeIds = (await session.Events.FetchStreamAsync(evt.TaskId))
            .Select(e => e.Data)
            .Where(e => e is TaskAssignedToUser)
            .Cast<TaskAssignedToUser>()
            .Select(e => e.UserId);
        var assignees = await session.LoadManyAsync<UserDetail>(assigneeIds);
        return new TaskArchive(task.Id, task.Title, task.Priority, assignees.Select(a => a.Name).OrderBy(a => a).ToArray());
    }
}