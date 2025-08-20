using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;
public class LogsController : Controller
{
    private readonly IUserLogService _userLogService;
    public LogsController(IUserLogService userService) => _userLogService = userService;

    [HttpGet]
    public async Task<ViewResult> List(int page = 1, int pageSize = 20)
    {
        var logs = await _userLogService.GetAllAsync();
        var totalCount = logs.Count;
        var pagedLogs = await _userLogService.GetPagedAsync(page, pageSize);

        var items = pagedLogs.Select(p => new UserLogEntryItemViewModel
        {
            Id = p.Id,
            UserId = p.UserId,
            Action = p.Action,
            Change = p.Change,
            TimeStamp = p.TimeStamp,
        });

        var model = new UserLogEntryViewModel
        {
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items.ToList()
        };

        return View(model);
    }

    [HttpGet("/ViewLog/{id}")]
    public async Task<ActionResult> ViewLog(long id)
    {
        var log = await _userLogService.FilterAllByIdAsync(id);

        if (log == null)
            return NotFound();

        var model = new UserLogEntryItemViewModel
        {
            Id = log.Id,
            UserId = log.UserId,
            Owner = "Admin",
            Action = log.Action,
            Change = log.Change,
            TimeStamp = log.TimeStamp,
        };

        return View(model);
    }

    [HttpGet("Logs/UsersLogs/{userId}")]
    public async Task<ActionResult> UsersLogs(long userId)
    {
        var logs = await _userLogService.FilterAllByUserIdAsync(userId);

        if (logs == null)
            return NotFound();

        var items = logs.Select(l => new UserLogEntryItemViewModel
        {
            Id = l.Id,
            UserId = l.UserId,
            Owner = l.Owner,
            Action = l.Action,
            Change = l.Change,
            TimeStamp = l.TimeStamp,
        });

        var model = new UserLogEntryViewModel
        {
            Items = items.ToList()
        };

        return View(model);
    }
}
