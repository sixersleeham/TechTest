using System;
using System.Linq;
using System.Text;
using UserManagement.Models;
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
    public ViewResult List(string? status)
    {
        var users = (status == "active" || status == "inactive") ?
            _userService.FilterByActive(status == "active") : _userService.GetAll();

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
            Items = items.ToList()
        };

        return View(model);
    }

    [HttpGet("users/viewuser/{id}")]
    public IActionResult ViewUser(long id)
    {
        var model = GetUserModelById(id);
        var logs = _userLogService.FilterAllByUserId(id);

        if (model == null)
            return NotFound();

        AddNewLog(model.Id, "View", "N/A");

        var userDetails = new UserDetailsViewModel
        {
            User = model,
            Logs = GetLogEntryModelById(id)
        };

        return View(userDetails);
    }

    [HttpGet("edit/{id}")]
    public IActionResult Edit(long id)
    {
        var model = GetUserModelById(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost("edit/{id}")]
    public IActionResult Edit(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = _userService.GetAll().SingleOrDefault(u => u.Id == model.Id);

        if (existingUser == null)
            return NotFound();

        string changeLog = BuildEditChangeLog(existingUser, model);

        existingUser.Forename = model.Forename;
        existingUser.Surname = model.Surname;
        existingUser.Email = model.Email;
        existingUser.IsActive = model.IsActive;
        existingUser.DateOfBirth = model.DateOfBirth;

        bool wasUpdated = _userService.UpdateUser(existingUser);

        if(wasUpdated)
            AddNewLog(model.Id, "Edit", changeLog);

        return Redirect("/users");
    }

    [HttpGet("delete/{id}")]
    public IActionResult Delete(long id)
    {
        var model = GetUserModelById(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost("delete/{id}")]
    public IActionResult DeleteUser(long id)
    {
        bool wasDeleted = _userService.DeleteUser(id);

        if(wasDeleted)
            AddNewLog(id, "Delete", "User Deleted");

        return Redirect("/users");
    }

    [HttpGet("add")]
    public ViewResult Add() => View();

    [HttpPost("add")]
    public IActionResult Add(UserListItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var maxId = (_userService.GetAll().Max(u => (long?)u.Id) ?? 0) + 1;

        var newUser = new User
        {
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = true,
            DateOfBirth = model.DateOfBirth
        };


        bool wasCreated = _userService.AddUser(newUser);

        if(wasCreated)
            AddNewLog(maxId, "Add", $"Added User {model.Forename} {model.Surname}");

        return Redirect("/users");
    }

    private UserListItemViewModel? GetUserModelById(long id)
    {
        var user = _userService.GetAll().FirstOrDefault(p => p.Id == id);

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

    private List<UserLogEntryItemViewModel> GetLogEntryModelById(long id)
    {
        var logs = _userLogService.FilterAllByUserId(id) ?? Enumerable.Empty<Log>();

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

    private void AddNewLog(long userId, string action, string change)
    {
        var newLog = new Log
        {
            UserId = userId,
            Owner = "Admin",
            Action = action,
            Change = change,
        };

        _userLogService.AddLog(newLog);
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
