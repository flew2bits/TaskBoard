using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskBoard.Users;
using TaskBoard.Users.Views;
using Wolverine;

namespace TaskBoard.Pages;

public class Users : PageModel
{
    public UserDetail[] UserDetails { get; set; } = Array.Empty<UserDetail>();

    public void OnGet([FromServices] IQuerySession session)
    {
        UserDetails = session.Query<UserDetail>().OrderBy(u => u.Name).ToArray();
    }

    public async Task<IActionResult> OnPostDeleteUser(Guid userId, [FromServices] IMessageBus bus)
    {
        await bus.InvokeAsync(new DeleteUser(userId));
        return RedirectToPage();
    }
}