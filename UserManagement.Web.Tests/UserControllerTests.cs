using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List("");

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Fact]
    public void ViewUser_WhenServiceReturnsUserDetailsModel_ModelMustContainerUserWithMatchingId()
    {
        var controller = CreateController();
        var users = SetupUsers();

        var result = controller.ViewUser(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.User.Id);
        Assert.Equal("juser@example.com", model.User.Email);
    }

    [Fact]
    public void ViewUser_WhenServiceReturnsUserDetailsModel_ShouldReturnLogsThatCorrespondToUser()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = controller.ViewUser(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDetailsViewModel>(viewResult.Model);

        Assert.All(model.Logs, log => Assert.Equal(1, log.UserId));
    }

    [Fact]
    public void ViewUser_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = controller.ViewUser(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void EditGet_WhenServiceReturnsUser_ModelMustContainUser()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Edit(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public void EditGet_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = controller.Edit(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void EditPost_WhenUserExistsInDatabaseAndCorrectModel_ShouldUpdateUserInDatabase()
    {
        var users = SetupUsers();
        var controller = CreateController();

        UserListItemViewModel newUser = new UserListItemViewModel()
        {
            Id = 1,
            Forename = "EditForname",
            Surname = "EditSurname",
            Email = "Edit@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        controller.Edit(newUser);

        var updatedUser = users.SingleOrDefault(u => u.Id == 1);

        updatedUser?.Forename.Should().Be("EditForname");
        updatedUser?.Surname.Should().Be("EditSurname");
        updatedUser?.Email.Should().Be("Edit@email");
        updatedUser?.IsActive.Should().BeTrue();
        updatedUser?.DateOfBirth.Should().Be(new DateTime(2025, 01, 01));
    }

    [Fact]
    public void EditPost_WhenIncorrectModel_ShouldReturnUserListItemViewModelType()
    {
        var controller = CreateController();

        UserListItemViewModel user = new UserListItemViewModel()
        {
            Id = 1,
            Forename = "",
            Surname = "Surname",
            Email = "mail@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        controller.ModelState.AddModelError("Forename", "Required");

        var result = controller.Edit(user);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);
    }

    [Fact]
    public void EditPost_WhenUserDoesNotExist_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        UserListItemViewModel user = new UserListItemViewModel()
        {
            Id = 11,
            Forename = "Forename",
            Surname = "Surname",
            Email = "mail@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        var result = controller.Edit(user);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void EditPost_WhenUserIsUpdated_ShouldAddEditLog()
    {
        // Arrange
        var model = new UserListItemViewModel
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            Email = "updated@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1990, 01, 01)
        };

        var existingUser = new User
        {
            Id = 1,
            Forename = "Original",
            Surname = "User",
            Email = "original@example.com",
            IsActive = false,
            DateOfBirth = new DateTime(1980, 01, 01)
        };

        Log? capturedLog = null;


        _userService.Setup(s => s.GetAll()).Returns(new[] { existingUser });
        _userService.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(true);
        _userLogService.Setup(s => s.AddLog(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        controller.Edit(model);

        Assert.NotNull(capturedLog);
        Assert.Equal(model.Id, capturedLog.UserId);
        Assert.Equal("Edit", capturedLog.Action);
    }


    [Fact]
    public void DeleteGet_WhenServiceReturnsUser_ModelMustContainUser()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Delete(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public void DeleteGet_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = controller.Delete(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeletePost_WhenUserExistInDatabase_ShouldRemoveUserFromDatabase()
    {

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "DeleteUserTest")
            .Options;

        using var context = new DataContext(options);

        var service = new UserService(context);


        var userToRemain = new User
        {
            Id = 1,
            Forename = "Nick",
            Surname = "Cage",
            Email = "nick@cage",
            IsActive = true,
            DateOfBirth = new DateTime(2012, 01, 01)
        };

        var userToDelete = new User
        {
            Id = 2,
            Forename = "Nick",
            Surname = "Coppola",
            Email = "nick@Coppola",
            IsActive = true,
            DateOfBirth = new DateTime(2012, 01, 01)
        };

        UsersController tempController = new UsersController(service, _userLogService.Object);

        service.AddUser(userToRemain);
        service.AddUser(userToDelete);

        tempController.DeleteUser(2);

        var deletedUser = service.GetAll().SingleOrDefault(u => u.Id == 2);
        var remainingUser = service.GetAll().SingleOrDefault(u => u.Id == 1);

        Assert.Null(deletedUser);
        Assert.NotNull(remainingUser);
    }

    [Fact]
    public void DeletePost_WhenUserIsDeleted_ShouldAddDeleteLog()
    {
        // Arrange
        var users = SetupUsers();

        Log? capturedLog = null;

        _userService.Setup(u => u.DeleteUser(1)).Returns(true);
        _userLogService.Setup(s => s.AddLog(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        controller.DeleteUser(1);

        Assert.NotNull(capturedLog);
        Assert.Equal(1, capturedLog.UserId);
        Assert.Equal("Delete", capturedLog.Action);
    }

    [Fact]
    public void AddGet_WhenServiceReturns_ShouldOnlyReturnViewResult()
    {
        var controller = CreateController();
        var result = controller.Add();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void AddPost_WhenValidModelIsPassed_UserAddedToDatabase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "AddUserTest")
            .Options;

        using var context = new DataContext(options);

        var service = new UserService(context);

        UsersController controller = new UsersController(service, _userLogService.Object);

        UserListItemViewModel model = new UserListItemViewModel
        {
            Id = 1,
            Forename = "Nick",
            Surname = "Cage",
            Email = "nick@cage",
            IsActive = true,
            DateOfBirth = new DateTime(2012, 01, 01)
        };

        controller.Add(model);

        var user = service.GetAll().SingleOrDefault(u => u.Id == 1);

        Assert.NotNull(user);        
    }

    [Fact]
    public void AddPost_WhenIncorrectModel_ShouldReturnUserListItemViewModelType()
    {
        var controller = CreateController();

        UserListItemViewModel user = new UserListItemViewModel()
        {
            Id = 1,
            Forename = "",
            Surname = "Surname",
            Email = "mail@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        controller.ModelState.AddModelError("Forename", "Required");

        var result = controller.Add(user);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);
    }

    [Fact]
    public void AddPost_WhenUserIsDeleted_ShouldAddLog()
    {
        UserListItemViewModel user = new UserListItemViewModel()
        {
            Id = 1,
            Forename = "",
            Surname = "Surname",
            Email = "mail@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        Log? capturedLog = null;

        _userService.Setup(u => u.AddUser(It.IsAny<User>())).Returns(true);
        _userLogService.Setup(s => s.AddLog(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        controller.Add(user);

        Assert.NotNull(capturedLog);
        Assert.Equal(1, capturedLog.UserId);
        Assert.Equal("Add", capturedLog.Action);
    }

    private User[] SetupUsers(long id = 1, string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Id = id,
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        var logs = new[]
        {
            new Log
            {
                Id = 1,
                UserId = id,
                Owner = "Admin",
                Action = "Add",
                Change = "Added User",
                TimeStamp = new DateTime(2000, 01, 01)
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        _userLogService
            .Setup(s => s.GetAll())
            .Returns(logs);

        _userLogService
            .Setup(s => s.FilterAllByUserId(id))
            .Returns(logs);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserLogService> _userLogService = new();
    private UsersController CreateController() => new(_userService.Object, _userLogService.Object);
}
