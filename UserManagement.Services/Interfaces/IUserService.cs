using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;
using ValidationResult = UserManagement.Service.Results.ValidationResult;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService 
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<List<User>> FilterByActiveAsync(bool isActive);
    Task<User?> FilterByIdAsync(long id);
    Task<List<User>> GetAllAsync();
    Task<ValidationResult> AddUserAsync(User user);
    Task<ValidationResult> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(long id);
}
