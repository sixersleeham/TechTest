using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;


namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    public UserService(IDataContext dataAccess) => _dataAccess = dataAccess;

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public async Task<List<User>> FilterByActiveAsync(bool isActive)
    {
        var users = await _dataAccess.GetAllAsync<User>();
        return users.Where(p => p.IsActive == isActive).ToList();
    }

    public async Task<User?> FilterByIdAsync(long id)
    {
        var users = await _dataAccess.GetAllAsync<User>();
        return users.SingleOrDefault(p => p.Id == id);
    }

    public async Task<List<User>> GetAllAsync()
    {
       var users = await _dataAccess.GetAllAsync<User>();
       return users.ToList();
    }

    public async Task<bool> AddUserAsync(User? user)
    {
        if(user == null)
            throw new ArgumentNullException(nameof(user));

        var context = new ValidationContext(user);
        Validator.ValidateObject(user, context, validateAllProperties: true);

        var existingUsers = await _dataAccess.GetAllAsync<User>();
        var existingUser = existingUsers
        .Where(u => u.Email == user.Email)
        .FirstOrDefault();

        if (existingUser != null)
            throw new ValidationException("Email already exists.");

        await _dataAccess.Create(user);

        return true;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var context = new ValidationContext(user);
        Validator.ValidateObject(user, context, validateAllProperties: true);

        var existingUsers = await _dataAccess.GetAllAsync<User>();
        var existingUser = existingUsers?.SingleOrDefault(u => u.Id == user.Id);

        if (existingUser == null)
            throw new ArgumentException($"User with ID {user.Id} not found.");

        var existingEmail = existingUsers?.SingleOrDefault(u => u.Email == user.Email && u.Id != user.Id);

        if (existingEmail != null)
            throw new ValidationException("Email already exists.");

        existingUser.Forename = user.Forename;
        existingUser.Surname = user.Surname;
        existingUser.Email = user.Email;
        existingUser.IsActive = user.IsActive;
        existingUser.DateOfBirth = user.DateOfBirth;

        await _dataAccess.Update(user);

        return true;
    }

    public async Task<bool> DeleteUserAsync(long id)
    {
        var users = await _dataAccess.GetAllAsync<User>();
        var user = users?.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {id} not found.");
        }

        await _dataAccess.Delete(user);

        return true;
    }
}
