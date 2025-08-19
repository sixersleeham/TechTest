using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        return _dataAccess.GetAll<User>().Where(p => p.IsActive == isActive);
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public bool AddUser(User? user)
    {
        if(user == null)
            throw new ArgumentNullException(nameof(user));

        var context = new ValidationContext(user);
        Validator.ValidateObject(user, context, validateAllProperties: true);

        var existingUser = _dataAccess.GetAll<User>()
        .SingleOrDefault(u => u.Email == user.Email);

        if (existingUser != null)
            throw new ValidationException("Email already exists.");

        _dataAccess.Create(user);

        return true;
    }

    public bool UpdateUser(User user)
    {
        var allUsers = GetAll();

        var context = new ValidationContext(user);
        Validator.ValidateObject(user, context, validateAllProperties: true);

        var existingUser = allUsers.SingleOrDefault(u => u.Id == user.Id);

        if (existingUser == null)
            throw new ArgumentException($"User with ID {user.Id} not found.");

        var existingEmail = allUsers.SingleOrDefault(u => u.Email == user.Email && u.Id != user.Id);

        if (existingEmail != null)
            throw new ValidationException("Email already exists.");

        existingUser.Forename = user.Forename;
        existingUser.Surname = user.Surname;
        existingUser.Email = user.Email;
        existingUser.IsActive = user.IsActive;
        existingUser.DateOfBirth = user.DateOfBirth;

        _dataAccess.Update(user);

        return true;
    }

    public bool DeleteUser(long id)
    {
        var users = GetAll();
        var userToDelete = users.FirstOrDefault(p => p.Id == id);

        if (userToDelete == null)
        {
            throw new ArgumentException($"User with ID {id} not found.");
        }

        _dataAccess.Delete(userToDelete);

        return true;
    }
}
