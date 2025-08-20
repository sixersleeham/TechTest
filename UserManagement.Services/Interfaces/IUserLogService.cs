using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services.Interfaces;
public interface IUserLogService
{
    Task<List<Log>> GetAllAsync();
    Task<List<Log>> FilterAllByUserIdAsync(long id);
    Task<Log?> FilterAllByIdAsync(long id);
    Task<List<Log>> FilterAllByActionAsync(string action);
    Task AddLogAsync(Log log);
    Task<List<Log>> GetPagedAsync(int page, int pageSize);
}
