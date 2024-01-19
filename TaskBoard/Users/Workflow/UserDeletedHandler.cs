using Marten;
using TaskBoard.Tasks;
using TaskBoard.Tasks.Views;
using Wolverine;

namespace TaskBoard.Users.Workflow;

public static class UserDeletedHandler
{
    public static IEnumerable<object> Handle(UserDeleted evt, IDocumentSession session)
    {
        var interestingStates = new List<TaskState> { TaskState.New, TaskState.InProgress, TaskState.OnHold };

        var tasks = session.Query<TaskDetail>().ToList()
            .Where(td => td.AssigneeId == evt.Id && interestingStates.Contains(td.State)).ToArray();

        foreach (var task in tasks)
        {
            yield return new DeleteAssignee(task.Id);
        }
    }
}