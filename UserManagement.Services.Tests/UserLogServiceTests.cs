using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;
public class UserLogServiceTests
{
    [Fact]
    public async Task GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var logs = SetupLogs();

        // Act: Invokes the method under test with the arranged parameters.
        var result = await service.GetAllAsync();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(logs);
    }

    [Fact]
    public async Task AddLog_WhenCalled_ShouldAddNewLogToDatabase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "AddLogTest")
            .Options;

        using var context = new DataContext(options);

        var service = new UserLogService(context);

        var log = SetupSingleLog(99);

        await service.AddLogAsync(log);

        var result = await service.FilterAllByIdAsync(log.Id);
        result.Should().BeEquivalentTo(log);
    }

    [Fact]
    public async Task AddLog_WhenLogIsNull_ShouldThrowNullExceptionArgument()
    {
        var service = CreateService();

        Func<Task> action = async () => await service.AddLogAsync(null);

        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("log");
    }

    [Fact]
    public async Task AddLog_MissingRequiredFields_ShouldThrowValidationException()
    {
        var log = SetupSingleLog(action: string.Empty);

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ValidationException>(async () => await service.AddLogAsync(log));
        Assert.Contains("Action", ex.Message);
    }

    [Fact]
    public async Task FilterAllByAction_WhenContextReturnsEntities_ReturnsAllEntitiesWithRequestedAction()
    {
        List<Log> logs = new List<Log>
        {
            SetupSingleLog(id : 1, action: "Add"),
            SetupSingleLog(id : 2, action: "Add"),
            SetupSingleLog(id : 3, action: "View"),
            SetupSingleLog(id : 4, action: "Add"),
            SetupSingleLog(id : 5, action: "Delete"),
            SetupSingleLog(id : 6, action: "View"),
            SetupSingleLog(id : 7, action: "Add"),
        };

        var service = CreateService();

        _dataContext.Setup(d => d.GetAllAsync<Log>()).ReturnsAsync(logs);

        var result = await service.FilterAllByActionAsync("Add");
        Assert.All(result, log => Assert.Equal("Add", log.Action));
    }

    [Fact]
    public async Task FilterAllByAction_WithEmptyAction_ShouldThrowArgumentException()
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await service.FilterAllByActionAsync(""));
        Assert.Contains("Action must be provided", ex.Message);
    }

    [Fact]
    public async Task GetAllByUserId_WhenContextReturnsEntities_ReturnsAllEntitiesWithRequestedAction()
    {
        List<Log> logs = new List<Log>
        {
            SetupSingleLog(id : 2, userId: 1),
            SetupSingleLog(id : 3, userId: 1),
            SetupSingleLog(id : 4, userId: 1),
            SetupSingleLog(id : 5, userId: 2),
            SetupSingleLog(id : 6, userId: 3),
            SetupSingleLog(id : 7, userId: 1),
            SetupSingleLog(id : 1, userId: 6),
        };
        
        var service = CreateService();
       
        _dataContext.Setup(d => d.GetAllAsync<Log>()).ReturnsAsync(logs);
       
        var result = await service.FilterAllByUserIdAsync(1);
        Assert.All(result, log => Assert.Equal(1, log.UserId));
    }

    [Fact]
    public async Task GetPaged_WhenContextReturnsEntities_ReturnsOnlyEntitiesWithinPageBounds()
    {
        var logs = SetupLogList(50);

        var service = CreateService();

        _dataContext.Setup(d => d.GetAllAsync<Log>()).ReturnsAsync(logs);

        var result = await service.GetPagedAsync(2, 20);

        Assert.Equal(20, result.Count());
        Assert.Contains(result, log => log.UserId == 30);
        Assert.True(result.SequenceEqual(result.OrderByDescending(l => l.TimeStamp)));
    }

    [Fact]
    public async Task GetPaged_WithInvalidPageOrSize_ShouldThrowArgumentException()
    {
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetPagedAsync(0, 10));
        Assert.Contains("Page", ex.Message);
    }

    private List<Log> SetupLogs(long id = 1, long userId = 1, string owner = "Admin", string action = "Add", string change = "Added new user")
    {
        var logs = new List<Log>
        {
            new Log
            {
              Id = id,
              UserId = userId,
              Owner = owner,
              Action = action,
              Change = change,
              TimeStamp = DateTime.Now,
            }
        };

        _dataContext
            .Setup(s => s.GetAllAsync<Log>())
            .ReturnsAsync(logs);

        return logs;
    }

    private Log SetupSingleLog(long id = 1, long userId = 1, string owner = "Admin", string action = "Add", string change = "Added new user")
    {
        return new Log
        {
            Id = id,
            UserId = userId,
            Owner = owner,
            Action = action,
            Change = change,
            TimeStamp = DateTime.Now,
        };
    }

    private List<Log> SetupLogList(int amount)
    {
        List<Log> logs = new List<Log>();

        for (int i = 1; i <= amount; i++)
        {
            logs.Add(SetupSingleLog(id: i, userId: i));
        }

        return logs;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserLogService CreateService() => new(_dataContext.Object);
}
