using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    public UserListItemViewModel User { get; set; } = default!;
    public List<UserLogEntryItemViewModel> Logs {  get; set; } = new();
}
