namespace UserManagement.WebMS.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("List", "Users");
}
