using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;
public class Log
{
    [Required]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required(ErrorMessage = "User Id is required")]
    public long UserId { get; set; }

    [Required(ErrorMessage = "Owner is required")]
    [MinLength(1, ErrorMessage = "Owner cannot be empty")]
    public string Owner { get; set; } = string.Empty;

    [Required(ErrorMessage = "Action is required")]
    [MinLength(1, ErrorMessage = "Action cannot be empty")]
    public string Action { get; set; } = string.Empty;

    [Required(ErrorMessage = "Change is required")]
    [MinLength(1, ErrorMessage = "Change cannot be empty")]
    public string Change { get; set; } = string.Empty;

    [Required(ErrorMessage = "Timestamp is required")]
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}
