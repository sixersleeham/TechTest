using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public async Task GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com"
        };
        await context.Create(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = await context.GetAllAsync<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public async Task GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        await context.Delete(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = await context.GetAllAsync<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public async Task GetAll_WhenCalled_ShouldReturnQueryableOfCorrectType()
    {
        var context = CreateContext();
        var users = await context.GetAllAsync<Log>();

        users.Should().AllBeAssignableTo<Log>();
    }

    [Fact]
    public async Task GetAll_WhenCalledForLogs_ShouldReturnSeededLogs()
    {
        var context = CreateContext();
        var logs = await context.GetAllAsync<Log>();

        logs.Should().NotBeEmpty();
        logs.Should().Contain(l => l.Action == "Add");
    }


    [Fact]
    public async Task Update_WhenCalled_ShouldPersistChanges()
    {
        var context = CreateContext();

        var user = await context.GetAllAsync<User>();

        user.First().Forename = "Updated";
        await context.Update(user.First());

        var allUsers = await context.GetAllAsync<User>();
        var updatedUser = allUsers.Single(u => u.Id == user.First().Id);
        updatedUser.Forename.Should().Be("Updated");
    }

    [Fact]
    public async Task Create_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Func<Task> act = async () => await context.Create<User>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
           .WithParameterName("entity");
    }

    [Fact]
    public async Task Update_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Func<Task> act = async () => await context.Update<User>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
           .WithParameterName("entity");
    }

    [Fact]
    public async Task Delete_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Func<Task> act = async () => await context.Delete<User>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
           .WithParameterName("entity");
    }

    private DataContext CreateContext() => new();
}
