using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await service.GetAllAsync();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task AddUser_WhenCalled_ShouldSaveUserToDatabase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "AddUserTest")
            .Options;

        using var context = new DataContext(options);

        var service = new UserService(context);

        var userToAdd = SetupSingleUser(99);

        await service.AddUserAsync(userToAdd);

        var result = await service.FilterByIdAsync(userToAdd.Id);

        result.Should().BeEquivalentTo(userToAdd);
    }

    [Fact]
    public async Task AddUser_WhenUserIsNull_ShouldThrowNullExceptionArgument()
    {
        var service = CreateService();

        Func<Task> action = async () => await service.AddUserAsync(null);

        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("user");
    }

    [Fact]
    public async Task AddUser_MissingRequiredFields_ShouldThrowValidationException()
    {
        var user = SetupSingleUser(email: string.Empty);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.AddUserAsync(user));
        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task AddUser_EmailAlreadyExists_ShouldThrowValidationException()
    {
        List<User> MockList = new List<User>
        {
            SetupSingleUser()
        };

        var newUser = SetupSingleUser(10, "Nick", "Cage");

        _dataContext.Setup(d => d.GetAllAsync<User>()).ReturnsAsync(MockList);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.AddUserAsync(newUser));
        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task AddUser_IncorrectEmailFormat_ShouldThrowValidationException()
    {
        var user = SetupSingleUser(email: "roy");

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>( async () => await service.AddUserAsync(user));
        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task AddUser_ForenameContainsIncorrectCharacters_ThrowValidationException()
    {
        var user = SetupSingleUser(forename: "R0y");

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.AddUserAsync(user));
        Assert.Contains("Forename", ex.Message);
    }


    [Fact]
    public async Task UpdateUser_UserExists_ShouldUpdateAndSaveChanges()
    {
        var service = CreateService();
        long id = 12;

        var ExistingUser = new List<User>
        {
            SetupSingleUser(id)
        };

        var UpdatedUser = SetupSingleUser(id, "NewForename", "NewSurname", "new@email", true);

        _dataContext.Setup(s => s.GetAllAsync<User>()).ReturnsAsync(ExistingUser);

        await service.UpdateUserAsync(UpdatedUser);

        var user = ExistingUser.SingleOrDefault(u => u.Id == id);

        user?.Forename.Should().Be("NewForename");
        user?.Surname.Should().Be("NewSurname");
        user?.Email.Should().Be("new@email");
        user?.IsActive.Should().BeTrue();
        user?.DateOfBirth.Should().Be(new DateTime(2000, 01, 01));
    }

    [Fact]
    public async Task UpdateUser_UserNotExist_ShouldThrowArgumentException()
    {
        var service = CreateService();

        var user = SetupSingleUser();

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await service.UpdateUserAsync(user));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task UpdateUser_EmailAlreadyExists_ShouldThrowValidationException()
    {
        List<User> MockList = new List<User>
        {
            SetupSingleUser(),
            SetupSingleUser(2, "Nick", "Cage", "nick@cage.com")
        };

        var newUser = SetupSingleUser(1, "Roy", "Waller", "nick@cage.com");

        _dataContext.Setup(d => d.GetAllAsync<User>()).ReturnsAsync(MockList);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.UpdateUserAsync(newUser));
        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public async Task UpdateUser_IncorrectEmailFormat_ThrowValidationException()
    {
        List<User> Mocklist = new List<User>{
            SetupSingleUser()
        };

        _dataContext.Setup(d => d.GetAllAsync<User>()).ReturnsAsync(Mocklist);

        var service = CreateService();

        var existingUser = await service.FilterByIdAsync(1);

        if (existingUser != null)
        {
            existingUser.Email = "roy";

            var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.UpdateUserAsync(existingUser));
            Assert.Contains("Email", ex.Message);
        }
    }

    [Fact]
    public async Task UpdateUser_ForenameContainsIncorrectCharacters_ThrowValidationException()
    {
        List<User> Mocklist = new List<User>{
            SetupSingleUser()
        };

        _dataContext.Setup(d => d.GetAllAsync<User>()).ReturnsAsync(Mocklist);

        var service = CreateService();

        var existingUser = await service.FilterByIdAsync(1);

        if (existingUser != null)
        {
            existingUser.Forename = "R0y";

            var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.UpdateUserAsync(existingUser));
            Assert.Contains("Forename", ex.Message);
        }
    }

    [Fact]
    public async Task DeleteUser_WhenUserExists_ShouldRemoveUserFromDatabase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "DeleteUserTest")
            .Options;

        using var context = new DataContext(options);

        var service = new UserService(context);

        var user = SetupSingleUser(email: "delete@email.com");
        var secondUser = SetupSingleUser(2, "Nick", "Cage", "NickCage@cage.com");

        await service.AddUserAsync(user);
        await service.AddUserAsync(secondUser);

        await service.DeleteUserAsync(user.Id);

        var deletedUser = await service.FilterByIdAsync(user.Id);
        var activeUser = await service.FilterByIdAsync(secondUser.Id);
        Assert.Null(deletedUser);
        Assert.NotNull(activeUser);       
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotExist_ShouldThrowArgumentException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await service.DeleteUserAsync(1));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task FilterUserByActive_WhenContextReturnsEntities_ReturnsAllEntitiesWithRequestedActiveState()
    {
        List<User> userList = new List<User>
        {
            SetupSingleUser(id: 1, isActive: true),
            SetupSingleUser(id: 2, isActive: false),
            SetupSingleUser(id: 3, isActive: true),
            SetupSingleUser(id: 4, isActive: true),
            SetupSingleUser(id: 5, isActive: true),
            SetupSingleUser(id: 6, isActive: true),
            SetupSingleUser(id: 7, isActive: false),
            SetupSingleUser(id: 8, isActive: true),
            SetupSingleUser(id: 9, isActive: true),
            SetupSingleUser(id: 10, isActive: false),
        };

        var service = CreateService();

        _dataContext.Setup(d => d.GetAllAsync<User>()).ReturnsAsync(userList);

        var result = await service.FilterByActiveAsync(true);

        Assert.Equal(7, result.Count());
        Assert.All(result, user => Assert.True(user.IsActive));
    }

    private List<User> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new List<User>
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                DateOfBirth = new DateTime(2000, 01, 01)
            }
        };

        _dataContext
            .Setup(s => s.GetAllAsync<User>())
            .ReturnsAsync(users);

        return users;
    }

    private User SetupSingleUser(long id = 1, string forename = "Roy", string surname = "Waller", string email = "roy@waller.com", bool isActive = true)
    {
        return new User
        {
            Id = id,
            Forename = forename,
            Surname = surname,
            Email = email,
            IsActive = isActive,
            DateOfBirth = new DateTime(2000, 01, 01)
        };
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
