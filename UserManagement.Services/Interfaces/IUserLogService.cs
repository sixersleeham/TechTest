using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Services.Interfaces;
public interface IUserLogService
{
    IEnumerable<Log> GetAll();
    IEnumerable<Log> FilterAllByUserId(long id);
    IEnumerable<Log> FilterAllByAction(string action);
    void AddLog(Log log);
    IEnumerable<Log> GetPaged(int page, int pageSize);
}
