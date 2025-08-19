using System;
using System.Linq;
using FluentAssertions;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com"
        };
        context.Create(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void GetAll_WhenCalled_ShouldReturnQueryableOfCorrectType()
    {
        var context = CreateContext();
        var users = context.GetAll<Log>();

        users.Should().AllBeAssignableTo<Log>();
    }

    [Fact]
    public void GetAll_WhenCalledForLogs_ShouldReturnSeededLogs()
    {
        var context = CreateContext();
        var logs = context.GetAll<Log>();

        logs.Should().NotBeEmpty();
        logs.Should().Contain(l => l.Action == "Add");
    }


    [Fact]
    public void Update_WhenCalled_ShouldPersistChanges()
    {
        var context = CreateContext();
        var user = context.GetAll<User>().First();

        user.Forename = "Updated";
        context.Update(user);

        var updatedUser = context.GetAll<User>().Single(u => u.Id == user.Id);
        updatedUser.Forename.Should().Be("Updated");
    }

    [Fact]
    public void Create_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Action act = () => context.Create<User>(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("entity");
    }

    [Fact]
    public void Update_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Action act = () => context.Update<User>(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("entity");
    }

    [Fact]
    public void Delete_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        var context = CreateContext();
        Action act = () => context.Delete<User>(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("entity");
    }

    private DataContext CreateContext() => new();
}
