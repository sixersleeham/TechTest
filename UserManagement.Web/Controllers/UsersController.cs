using System;
using System.Linq;
using AspNetCoreGeneratedDocument;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("")]
    public ViewResult List(string? status)
    {
        var users = !string.IsNullOrEmpty(status) ? _userService.FilterByActive(status == "active") : _userService.GetAll();

        var model = GetModelFromUserList(users);

        return View(model);
    }

    [HttpGet("viewuser/{id}")]
    public ViewResult ViewUser(int id)
    {
        var user = _userService.GetAll().Where(p => p.Id == id);

        var model = GetModelFromUserList(user);

        return View(model);
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
            Id = maxId,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = true,
            DateOfBirth = model.DateOfBirth
        };

        _userService.AddUser(newUser);

        return Redirect("/users");
    }

    private UserListViewModel GetModelFromUserList(IEnumerable<Models.User> users)
    {
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

        return model;
    }    
}
