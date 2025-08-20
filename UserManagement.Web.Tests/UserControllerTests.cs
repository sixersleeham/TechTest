using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using UserManagement.Service.Results;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await controller.List("");

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task ViewUser_WhenServiceReturnsUserDetailsModel_ModelMustContainerUserWithMatchingId()
    {
        var controller = CreateController();
        var users = SetupUsers();

        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(users.First);

        var result = await controller.ViewUser(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.User.Id);
        Assert.Equal("juser@example.com", model.User.Email);
    }

    [Fact]
    public async Task ViewUser_WhenServiceReturnsUserDetailsModel_ShouldReturnLogsThatCorrespondToUser()
    {
        var users = SetupUsers();
        var controller = CreateController();

        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(users.First);

        var result = await controller.ViewUser(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserDetailsViewModel>(viewResult.Model);

        Assert.All(model.Logs, log => Assert.Equal(1, log.UserId));
    }

    [Fact]
    public async Task ViewUser_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = await controller.ViewUser(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditGet_WhenServiceReturnsUser_ModelMustContainUser()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(users.First);

        // Act: Invokes the method under test with the arranged parameters.
        var result = await controller.Edit(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task EditGet_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = await controller.Edit(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_WhenUserExistsInDatabaseAndCorrectModel_ShouldUpdateUserInDatabase()
    {
        var users = SetupUsers();
        var controller = CreateController();

        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(users.First);
        _userService.Setup(u => u.UpdateUserAsync(It.Is<User>(u => u.Email.Contains("@")))).ReturnsAsync(ValidationResult.Success);

        UserListItemViewModel newUser = new UserListItemViewModel()
        {
            Id = 1,
            Forename = "EditForname",
            Surname = "EditSurname",
            Email = "Edit@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        await controller.Edit(newUser);

        var updatedUser = users.SingleOrDefault(u => u.Id == 1);

        updatedUser?.Forename.Should().Be("EditForname");
        updatedUser?.Surname.Should().Be("EditSurname");
        updatedUser?.Email.Should().Be("Edit@email");
        updatedUser?.IsActive.Should().BeTrue();
        updatedUser?.DateOfBirth.Should().Be(new DateTime(2025, 01, 01));
    }

    [Fact]
    public async Task EditPost_WhenIncorrectModel_ShouldReturnUserListItemViewModelType()
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

        var result = await controller.Edit(user);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task EditPost_WhenUserDoesNotExist_ThrowNotFound()
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

        var result = await controller.Edit(user);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_WhenUserIsUpdated_ShouldAddEditLog()
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


        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(existingUser);
        _userService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(ValidationResult.Success);
        _userLogService.Setup(s => s.AddLogAsync(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        await controller.Edit(model);

        Assert.NotNull(capturedLog);
        Assert.Equal(model.Id, capturedLog.UserId);
        Assert.Equal("Edit", capturedLog.Action);
    }


    [Fact]
    public async Task DeleteGet_WhenServiceReturnsUser_ModelMustContainUser()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        _userService.Setup(u => u.FilterByIdAsync(1)).ReturnsAsync(users.First);

        // Act: Invokes the method under test with the arranged parameters.
        var result = await controller.Delete(1);

        // Assert: Verifies that the action of the method under test behaves as expected.
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task DeleteGet_WhenUserNotExistInDatabase_ThrowNotFound()
    {
        var users = SetupUsers();
        var controller = CreateController();

        var result = await controller.Delete(11);

        var viewResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePost_WhenUserExistInDatabase_ShouldRemoveUserFromDatabase()
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

        await service.AddUserAsync(userToRemain);
        await service.AddUserAsync(userToDelete);

        await tempController.DeleteUser(2);

        var deletedUser = await service.FilterByIdAsync(2);
        var remainingUser = await service.FilterByIdAsync(1);

        Assert.Null(deletedUser);
        Assert.NotNull(remainingUser);
    }

    [Fact]
    public async Task DeletePost_WhenUserIsDeleted_ShouldAddDeleteLog()
    {
        // Arrange
        var users = SetupUsers();

        Log? capturedLog = null;

        _userService.Setup(u => u.DeleteUserAsync(1)).ReturnsAsync(true);
        _userLogService.Setup(s => s.AddLogAsync(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        await controller.DeleteUser(1);

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
    public async Task AddPost_WhenValidModelIsPassed_UserAddedToDatabase()
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
            Email = "nickcage@email",
            IsActive = true,
            DateOfBirth = new DateTime(2012, 01, 01)
        };

        await controller.Add(model);

        var user = service.FilterByIdAsync(1);

        Assert.NotNull(user);        
    }

    [Fact]
    public async Task AddPost_WhenIncorrectModel_ShouldReturnUserListItemViewModelType()
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

        var result = await controller.Add(user);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserListItemViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task AddPost_WhenUserIsDeleted_ShouldAddLog()
    {
        var users = SetupUsers();

        UserListItemViewModel user = new UserListItemViewModel()
        {
            Id = 2,
            Forename = "",
            Surname = "Surname",
            Email = "mail@email",
            IsActive = true,
            DateOfBirth = new DateTime(2025, 01, 01)
        };

        Log? capturedLog = null;

        _userService.Setup(u => u.AddUserAsync(It.IsAny<User>())).ReturnsAsync(ValidationResult.Success);
        _userLogService.Setup(s => s.AddLogAsync(It.IsAny<Log>())).Callback<Log>(log => capturedLog = log);

        var controller = CreateController();

        await controller.Add(user);

        Assert.NotNull(capturedLog);
        Assert.Equal(2, capturedLog.UserId);
        Assert.Equal("Add", capturedLog.Action);
    }

    private List<User> SetupUsers(long id = 1, string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new List<User>
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

        var logs = new List<Log>
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
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(users);

        _userLogService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(logs);

        _userLogService
            .Setup(s => s.FilterAllByUserIdAsync(id))
            .ReturnsAsync(logs);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserLogService> _userLogService = new();
    private UsersController CreateController() => new(_userService.Object, _userLogService.Object);
}
