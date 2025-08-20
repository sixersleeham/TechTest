using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Service.Results;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Logs;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserLogService _userLogService;
    public UsersController(IUserService userService, IUserLogService userLogService)
    {
        _userService = userService;
        _userLogService = userLogService;
    }

    [HttpGet("")]
    public async Task<ViewResult> List(string? status)
    {
        Task<List<User>> userTask = (status == "active" || status == "inactive") ?
            _userService.FilterByActiveAsync(status == "active") : _userService.GetAllAsync();

        var users = await userTask;

        var items = users.Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            IsActive = p.IsActive,
            DateOfBirth = p.DateOfBirth
        });

        var model = new UserListViewModel
        {
            Items = items.OrderBy(p => p.Id).ToList(),
        };

        return View(model);
    }

    [HttpGet("users/viewuser/{id}")]
    public async Task<IActionResult> ViewUser(long id)
    {
        var model = await GetUserModelById(id);
        var logs = await _userLogService.FilterAllByUserIdAsync(id);

        if (model == null)
            return NotFound();

        AddNewLog(model.Id, "View", "N/A");

        var userDetails = new UserDetailsViewModel
        {
            User = model,
            Logs = await GetLogEntryModelById(id)
        };

        return View(userDetails);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(long id)
    {
        var model = await GetUserModelById(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userService.FilterByIdAsync(model.Id);

        if (existingUser == null)
            return NotFound();

        string changeLog = BuildEditChangeLog(existingUser, model);

        existingUser.Forename = model.Forename;
        existingUser.Surname = model.Surname;
        existingUser.Email = model.Email;
        existingUser.IsActive = model.IsActive;
        existingUser.DateOfBirth = model.DateOfBirth;

        ValidationResult validationResult = await _userService.UpdateUserAsync(existingUser);

        if(validationResult.IsValid)
            AddNewLog(model.Id, "Edit", changeLog);
        else
        {
            ModelState.AddModelError(string.Empty, validationResult.ErrorMessage ?? "An unknown error occurred.");
            return View();
        }

        return Redirect("/users");
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var model = await GetUserModelById(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        bool wasDeleted = await _userService.DeleteUserAsync(id);

        if(wasDeleted)
            AddNewLog(id, "Delete", "User Deleted");

        return Redirect("/users");
    }

    [HttpGet("add")]
    public ViewResult Add() => View();

    [HttpPost("add")]
    public async Task<IActionResult> Add(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var users = await _userService.GetAllAsync();
        var maxId = (users.Max(u => (long?)u.Id) ?? 0) + 1;

        var newUser = new User
        {
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = true,
            DateOfBirth = model.DateOfBirth
        };


        ValidationResult validationResult = await _userService.AddUserAsync(newUser);

        if (validationResult.IsValid)
        {
            AddNewLog(maxId, "Add", $"Added User {model.Forename} {model.Surname}");
        }
        else
        {
            ModelState.AddModelError(string.Empty, validationResult.ErrorMessage ?? "An unknown error occurred.");
            return View();
        }

        return Redirect("/users");
    }

    private async Task<UserListItemViewModel?> GetUserModelById(long id)
    {
        var user = await _userService.FilterByIdAsync(id);

        if (user == null)
            return null;

        var item = new UserListItemViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive,
            DateOfBirth = user.DateOfBirth
        };

        return item;
    }

    private async Task<List<UserLogEntryItemViewModel>> GetLogEntryModelById(long id)
    {
        var logs = await _userLogService.FilterAllByUserIdAsync(id) ?? Enumerable.Empty<Log>();

        return logs.Select(log => new UserLogEntryItemViewModel
        {
            Id = log.Id,
            UserId = log.UserId,
            Owner = "Admin",
            Action = log.Action,
            Change = log.Change,
            TimeStamp = log.TimeStamp
        }).ToList();
    }

    private async void AddNewLog(long userId, string action, string change)
    {
        var newLog = new Log
        {
            UserId = userId,
            Owner = "Admin",
            Action = action,
            Change = change,
        };

        await _userLogService.AddLogAsync(newLog);
    }

    private string BuildEditChangeLog(User currentUser, UserListItemViewModel updatedUser)
    {
        var sb = new StringBuilder();

        if (currentUser.Forename != updatedUser.Forename)
            sb.AppendLine($"Forename changed from {currentUser.Forename} to {updatedUser.Forename}");

        if (currentUser.Surname != updatedUser.Surname)
            sb.AppendLine($"Surename changed from {currentUser.Surname} to {updatedUser.Surname}");

        if (currentUser.Email != updatedUser.Email)
            sb.AppendLine($"Email changed from {currentUser.Email} to {updatedUser.Email}");

        if (currentUser.IsActive != updatedUser.IsActive)
            sb.AppendLine($"IsActive changed from {currentUser.IsActive} to {updatedUser.IsActive}");

        if (currentUser.DateOfBirth != updatedUser.DateOfBirth)
            sb.AppendLine($"Date of Birth changed from {currentUser.DateOfBirth} to {updatedUser.DateOfBirth}");

        if (sb.Length == 0)
            return "No changes made";

        return sb.ToString();
    }
}
