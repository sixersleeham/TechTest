using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Users;

public class UserListViewModel
{
    public List<UserListItemViewModel> Items { get; set; } = new();
}

public class UserListItemViewModel
{
    public long Id { get; set; }
    [Required(ErrorMessage ="Forename is required")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Forename must contain only letters")]
    public string Forename { get; set; } = string.Empty;
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Surname must contain only letters")]
    [Required(ErrorMessage = "Surname is required")]
    public string Surname { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Wrong Email Format")]
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}
