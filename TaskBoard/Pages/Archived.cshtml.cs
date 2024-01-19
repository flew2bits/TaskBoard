using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskBoard.Tasks.Views;

namespace TaskBoard.Pages;

public class Archived : PageModel
{
    public TaskArchive[] Archives { get; set; } = Array.Empty<TaskArchive>();

    public void OnGet([FromServices] IQuerySession session)
    {
        Archives = session.Query<TaskArchive>().ToArray();
    }
}