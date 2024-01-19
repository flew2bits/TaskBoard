using Marten;
using Marten.Events.Projections;
using TaskBoard.Tasks.Views;

namespace TaskBoard.Tasks;

public static class Configuration
{
    public static IServiceCollection AddTasks(this IServiceCollection services) =>
        services.ConfigureMarten(opt =>
        {
            opt.Projections.Add<TaskDetailProjection>(ProjectionLifecycle.Inline);
            opt.Projections.Add<TaskArchiveProjection>(ProjectionLifecycle.Inline);
            opt.Projections.Add<TaskNotesProjection>(ProjectionLifecycle.Inline);
        });
}