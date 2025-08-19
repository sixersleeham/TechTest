using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;
public class Log
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string Change { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}
