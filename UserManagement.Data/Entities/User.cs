using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class User
{
    [Required]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required(ErrorMessage = "Forename is required")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Forename must contain only letters")]
    public string Forename { get; set; } = default!;

    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Surname must contain only letters")]
    [Required(ErrorMessage = "Surname is required")]
    public string Surname { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Wrong Email Format")]
    public string Email { get; set; } = default!;

    public bool IsActive { get; set; }

    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}
