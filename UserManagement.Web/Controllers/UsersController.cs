using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public ViewResult List(string? status)
    {
        var users = _userService.GetAll();

        if (!string.IsNullOrEmpty(status)){
            status = status.ToLower();
            users = status switch
            {
                "active" => users.Where(p => p.IsActive),
                "inactive" => users.Where(p => !p.IsActive),
                _ => users
            };
        }

        var items = users.Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            IsActive = p.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return View(model);
    }

    

}
