using JasperFx.CodeGeneration;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Oakton;
using TaskBoard.Tasks;
using TaskBoard.Tasks.Hubs;
using TaskBoard.Users;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(opt =>
{
    opt.LocalQueue("ordered").ConfigureExecution(options =>
    {
        options.MaxDegreeOfParallelism = 1;
        options.EnsureOrdered = true;
    });
});
builder.Host.ApplyOaktonExtensions();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddMarten(opt =>
    {
        opt.Connection(builder.Configuration.GetConnectionString("marten") ??
                       throw new InvalidOperationException("No connection string"));
        //opt.GeneratedCodeMode = TypeLoadMode.Auto;
    })
    .AddAsyncDaemon(DaemonMode.Solo)
    .UseLightweightSessions()
    .IntegrateWithWolverine()
    .EventForwardingToWolverine();

builder.Services
    .AddUsers()
    .AddTasks();

builder.Services.AddSignalR();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapWolverineEndpoints();
app.MapHub<TaskHub>("/taskHub");

await app.RunOaktonCommands(args);