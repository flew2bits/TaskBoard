using Marten;
using Marten.Events.Projections;
using TaskBoard.Users.Views;

namespace TaskBoard.Users;

public static class Configuration
{
    public static IServiceCollection AddUsers(this IServiceCollection services) =>
        services.ConfigureMarten(opt =>
        {
            opt.Projections.Add<UserDetailProjection>(ProjectionLifecycle.Inline);
            //opt.Projections.Add<UserActiveTasksProjection>(ProjectionLifecycle.Async);
        });

}