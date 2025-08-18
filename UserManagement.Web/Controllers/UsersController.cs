using System;
using System.Linq;
using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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

    [HttpGet("viewuser/{id}")]
    public IActionResult ViewUser(long id)
    {
        var model = GetUserModelById(id);

        if (model == null)
            return NotFound();

        return View(model);
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

        var newUser = new User
        {
            Id = model.Id,
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            IsActive = model.IsActive,
            DateOfBirth = model.DateOfBirth
        };

        _userService.UpdateUser(newUser);

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
        var newUser = _userService.GetAll().FirstOrDefault(p => p.Id == id);

        if(newUser == null)
            return NotFound();

        _userService.DeleteUser(newUser);

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
}
