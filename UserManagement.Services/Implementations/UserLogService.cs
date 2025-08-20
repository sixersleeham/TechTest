using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Domain.Implementations;
public class UserLogService : IUserLogService
{
    private readonly IDataContext _dataAccess;
    public UserLogService(IDataContext dataAccess) => _dataAccess = dataAccess;

    public async Task AddLogAsync(Log? log)
    {
        if(log == null)
            throw new ArgumentNullException(nameof(log));

        Console.WriteLine($"Change: '{log.Change}'");
        Console.WriteLine($"Length: {log.Change?.Length}");


        var context = new ValidationContext(log);
        Validator.ValidateObject(log, context, validateAllProperties: true);

        await _dataAccess.Create(log);
    }

    public async Task<List<Log>> FilterAllByActionAsync(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action must be provided.", nameof(action));
        var logs = await _dataAccess.GetAllAsync<Log>();
        return logs.Where(p => p.Action == action).OrderByDescending(l => l.TimeStamp).ToList();
    }

    public async Task<Log?> FilterAllByIdAsync(long id) => await _dataAccess.GetAll<Log>().FirstOrDefaultAsync(p => p.Id == id);
    public async Task<List<Log>> FilterAllByUserIdAsync(long id)
    {
        var logs = await _dataAccess.GetAllAsync<Log>();
        return logs.Where(p => p.UserId == id).ToList();
    }
   
    public async Task<List<Log>> GetAllAsync() => await _dataAccess.GetAllAsync<Log>();

    public async Task<List<Log>> GetPagedAsync(int page, int pageSize) {

        if(page <= 0 || pageSize <= 0)
            throw new ArgumentException("Page and page size must be greater than 0");

        var pagedLogs = await _dataAccess.GetAllAsync<Log>();
        return pagedLogs.OrderByDescending(l => l.TimeStamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    }
}
