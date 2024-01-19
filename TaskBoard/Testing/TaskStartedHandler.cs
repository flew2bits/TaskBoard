using TaskBoard.Tasks;

namespace TaskBoard.Testing;

public static class TaskStartedHandler
{
    public static void Handle(TaskStarted evt)
    {
        Console.WriteLine($"A new task was started with title {evt.Title} and priority {evt.Priority}");
    }
}