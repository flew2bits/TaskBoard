namespace TaskBoard.Tasks;

public record TaskNote(Guid NoteId, string Text);
public record TaskAggregate(Guid Id, string Title, TaskPriority Priority, TaskState State, Guid? AssignedTo, TaskNote[] Notes)
{
    public static TaskAggregate Create(TaskStarted evt) => new(evt.TaskId, evt.Title, evt.Priority, TaskState.New, null, Array.Empty<TaskNote>());

    public TaskAggregate Apply(TaskAssignedToUser evt) =>
        this with { AssignedTo = evt.UserId };

    public TaskAggregate Apply(TaskUnassigned evt) => this with { AssignedTo = null };

    public TaskAggregate Apply(PriorityChanged evt) => this with { Priority = evt.Priority };

    public TaskAggregate Apply(WorkStarted evt) => this with { State = TaskState.InProgress };

    public TaskAggregate Apply(TaskHeld evt) => this with { State = TaskState.OnHold };

    public TaskAggregate Apply(TaskResumed evt) => this with { State = TaskState.InProgress };

    public TaskAggregate Apply(TaskRenamed evt) => this with { Title = evt.Title };

    public TaskAggregate Apply(TaskCompleted evt) => this with { State = TaskState.Completed };

    public TaskAggregate Apply(TaskCanceled evt) => this with { State = TaskState.Canceled };

    public TaskAggregate Apply(ConversationStarted evt) =>
        this with { Notes = Notes.Append(new TaskNote(evt.NoteId, evt.Note)).ToArray() };

    public TaskAggregate Apply(RespondedToNote evt) =>
        this with { Notes = Notes.Append(new TaskNote(evt.NoteId, evt.Text)).ToArray() };
}