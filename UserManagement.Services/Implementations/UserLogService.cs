using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Domain.Implementations;
public class UserLogService : IUserLogService
{
    private readonly IDataContext _dataAccess;
    public UserLogService(IDataContext dataAccess) => _dataAccess = dataAccess;

    public void AddLog(Log? log)
    {
        if(log == null)
            throw new ArgumentNullException(nameof(log));

        Console.WriteLine($"Change: '{log.Change}'");
        Console.WriteLine($"Length: {log.Change?.Length}");


        var context = new ValidationContext(log);
        Validator.ValidateObject(log, context, validateAllProperties: true);

        _dataAccess.Create(log);
    }

    public IEnumerable<Log> FilterAllByAction(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action must be provided.", nameof(action));

        return _dataAccess.GetAll<Log>().Where(p => p.Action == action).OrderByDescending(l => l.TimeStamp);
    }

    public IEnumerable<Log> FilterAllByUserId(long id) => GetAll().Where(p => p.UserId == id);
    public IEnumerable<Log> GetAll() => _dataAccess.GetAll<Log>();

    public IEnumerable<Log> GetPaged(int page, int pageSize) {

        if(page <= 0 || pageSize <= 0)
            throw new ArgumentException("Page and page size must be greater than 0");

        return GetAll().OrderByDescending(l => l.TimeStamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    }
}
