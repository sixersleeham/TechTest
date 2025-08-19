using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Interfaces;

namespace UserManagement.Services.Domain.Implementations;
internal class UserLogService : IUserLogService
{
    private readonly IDataContext _dataAccess;
    public UserLogService(IDataContext dataAccess) => _dataAccess = dataAccess;
    public void AddLog(Log log) => _dataAccess.Create(log);
    public IEnumerable<Log> FilterAllByAction(string action) => _dataAccess.GetAll<Log>().Where(p => p.Action == action).OrderByDescending(l => l.TimeStamp);
    public IEnumerable<Log> FilterAllByUserId(long id) => _dataAccess.GetAll<Log>().Where(p => p.UserId == id);
    public IEnumerable<Log> GetAll() => _dataAccess.GetAll<Log>();
    public List<Log> GetPaged(int page, int pageSize) {
        return GetAll().OrderByDescending(l => l.TimeStamp)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    }
}
