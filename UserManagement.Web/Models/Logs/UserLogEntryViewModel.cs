using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Logs;

public class UserLogEntryViewModel
{
    public List<UserLogEntryItemViewModel> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class UserLogEntryItemViewModel
{
    [Required]
    public long Id { get; set; }
    [Required]
    public long UserId { get; set; }
    [Required]
    public string? Action { get; set; }
    [Required]
    public string Owner { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    [Required]
    public DateTime TimeStamp { get; set; }
}


